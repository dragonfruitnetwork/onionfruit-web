#[repr(C, packed(1))]
pub struct InteropNetworkEntry  {
    pub network: [u8; 16],
    pub cidr: u8,

    pub country_code: [u8; 2]
}

#[repr(C, packed(1))]
pub struct InteropNetworkRange<T> {
    pub start: T,
    pub end: T,

    pub country_code: [u8; 2]
}

#[repr(C, packed(1))]
#[derive(Clone, Copy)]
pub struct InteropNetworkSortResult {
    pub v4entries: *mut InteropNetworkRange<u32>,
    pub v4count: usize,

    pub v6entries: *mut InteropNetworkRange<u128>,
    pub v6count: usize
}