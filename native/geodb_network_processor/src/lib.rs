mod interop_format;
mod netblock_ops;

use ipnetwork::Ipv6Network;
use rangemap::RangeInclusiveMap;
use interop_format::{InteropNetworkEntry, InteropNetworkRange};

use std::{slice, mem};
use std::net::Ipv6Addr;
use std::os::raw::c_void;

/// Represents a block of network addresses
/// For simplicity, all addresses are treated as IPv6
#[derive(Debug, Clone)]
struct NetBlock {
    pub net: Ipv6Network,
    pub cc: [u8; 2]
}

/// Represents the info associated with a network block.
#[derive(Copy, Clone, Eq, PartialEq, Debug)]
struct NetworkInfo {
    cc: [u8; 2]
}

#[no_mangle]
pub extern "cdecl" fn perform_network_sort(ptr: *const c_void, length: usize, range_length: *mut usize, range_capacity: *mut usize) -> *mut InteropNetworkRange {
    let mut data = Vec::<NetBlock>::with_capacity(length);
    let slice = unsafe { slice::from_raw_parts(ptr as *const InteropNetworkEntry, length) };

    for entry in slice {
        let result = Ipv6Network::new(Ipv6Addr::from(u128::from_be_bytes(entry.network)), entry.cidr);

        if let Ok(network) = result {
            data.push(NetBlock {
                cc: entry.country_code,
                net: network
            });
        }
    }

    // do sort before pushing to map
    data.sort();

    let mut map: RangeInclusiveMap<u128, NetworkInfo, _> = RangeInclusiveMap::new();    

    for item in data {
        let start = item.net.network();
        let end = item.net.broadcast();

        let info = NetworkInfo {
            cc: item.cc
        };

        map.insert(start.into()..=end.into(), info);
    }

    // collect results and expose entries to caller
    let mut result_listing = Vec::<InteropNetworkRange>::with_capacity(map.iter().count());
    
    for (range, info) in map.iter() {
        let info = InteropNetworkRange {
            start: *range.start(),
            end: *range.end(),
            cc: info.cc
        };

        result_listing.push(info);
    }

    unsafe {
        *range_length = result_listing.len();
        *range_capacity = result_listing.capacity();

        let bytes = result_listing.as_mut_ptr();
        mem::forget(result_listing);
    
        return bytes;
    }
}

#[no_mangle]
pub extern "cdecl" fn free_vec(ptr: *mut c_void, length: usize, capacity: usize) {
    let slice = unsafe { Vec::<InteropNetworkRange>::from_raw_parts(ptr as *mut InteropNetworkRange, length, capacity) };
    drop(slice);
}