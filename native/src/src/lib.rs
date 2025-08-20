mod interop_format;
mod netblock_ops;

use ipnetwork::Ipv6Network;
use rangemap::{RangeInclusiveMap, StepFns};
use interop_format::{InteropNetworkEntry, InteropNetworkRange, InteropNetworkSortResult};

use std::{slice, mem};
use std::net::Ipv6Addr;
use std::os::raw::c_void;

/// Represents a block of network addresses
/// For simplicity, all addresses are treated as IPv6
struct NetBlock {
    network: Ipv6Network,
    country_code: [u8; 2]
}

/// Represents the info associated with a network block.
#[derive(Copy, Clone, Eq, PartialEq)]
struct NetworkInfo {
    country_code: [u8; 2]
}

#[unsafe(no_mangle)]
pub extern "C" fn sort_network_entries(ptr: *const c_void, length: usize, out: *mut InteropNetworkSortResult) {
    let mut data = Vec::<NetBlock>::with_capacity(length);
    let slice = unsafe { slice::from_raw_parts(ptr as *const InteropNetworkEntry, length) };

    for entry in slice {
        let result = Ipv6Network::new(Ipv6Addr::from(u128::from_be_bytes(entry.network)), entry.cidr);

        if let Ok(network) = result {
            data.push(NetBlock {
                country_code: entry.country_code,
                network
            });
        }
    }

    // do sort before pushing to map
    data.sort();

    let mut v4map: RangeInclusiveMap<u32, NetworkInfo, _> = RangeInclusiveMap::new();    
    let mut v6map: RangeInclusiveMap<u128, NetworkInfo, _> = RangeInclusiveMap::new(); 

    for item in data {
        let v6start = item.network.network();
        let v6end = item.network.broadcast();
        let info = NetworkInfo {
            country_code: item.country_code
        };

        if let (Some(start), Some(end)) = (v6start.to_ipv4_mapped(), v6end.to_ipv4_mapped()) {
            v4map.insert(start.into()..=end.into(), info);
        }
        else {
            v6map.insert(v6start.into()..=v6end.into(), info);
        }
    }

    // collect results and expose entries to caller
    let mut v4results = create_network_list(v4map, |num: u32| -> u32 {
        return u32::from_ne_bytes(num.to_be_bytes());
    });

    let mut v6results = create_network_list(v6map, |num: u128| -> u128 {
        return u128::from_ne_bytes(num.to_be_bytes());
    });

    v4results.shrink_to_fit();
    v6results.shrink_to_fit();

    unsafe {
        *out = InteropNetworkSortResult {
            v4entries: v4results.as_mut_ptr(),
            v4capacity: v4results.capacity(),
            v4count: v4results.len(),

            v6entries: v6results.as_mut_ptr(),
            v6capacity: v6results.capacity(),
            v6count: v6results.len()
        };
    }

    mem::forget(v4results);
    mem::forget(v6results);
}

#[unsafe(no_mangle)]
pub unsafe extern "C" fn free_sort_result(ptr: *const InteropNetworkSortResult) {
    let result = unsafe { *ptr };
    let v4_vector = unsafe { Vec::<InteropNetworkRange<u32>>::from_raw_parts(result.v4entries, result.v4count, result.v4capacity) };
    let v6_vector = unsafe { Vec::<InteropNetworkRange<u128>>::from_raw_parts(result.v6entries, result.v6count, result.v6capacity) };

    mem::drop(v4_vector);
    mem::drop(v6_vector);
}

fn create_network_list<T>(map: RangeInclusiveMap<T, NetworkInfo>, range_value_selector: fn(T) -> T) -> Vec::<InteropNetworkRange<T>> where T : Copy, T : Eq, T : PartialEq, T : Ord, T : PartialOrd, T : StepFns<T> {
    let mut vec = Vec::<InteropNetworkRange<T>>::with_capacity(map.len());

    for (range, info) in map.iter() {
        let info = InteropNetworkRange::<T> {
            start: range_value_selector(*range.start()),
            end: range_value_selector(*range.end()),

            // clone values to prevent using references that could be disposed of by the caller.
            country_code: info.country_code.clone()
        };

        vec.push(info);
    }

    return vec;
}