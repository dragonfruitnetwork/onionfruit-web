#[repr(C)]
pub struct InteropNetworkEntry  {
    pub country_code: [u8; 2],
    pub network: [u8; 16],
    pub cidr: u8
}

#[repr(C)]
pub struct InteropNetworkRange<T> {
    pub start: T,
    pub end: T,
    pub country_code: [u8; 2]
}

#[repr(C)]
#[derive(Clone, Copy)]
pub struct InteropNetworkSortResult {
    pub v4entries: *mut InteropNetworkRange<u32>,
    pub v4count: usize,

    pub v6entries: *mut InteropNetworkRange<u128>,
    pub v6count: usize
}