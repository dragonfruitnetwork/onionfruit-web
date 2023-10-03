use std::ffi::CString;

#[repr(C)]
pub struct InteropNetworkEntry  {
    pub country_code: CString,
    pub network: [u8; 16],
    pub cidr: u8
}

#[repr(C)]
pub struct InteropNetworkRange {
    pub start: u128,
    pub end: u128,

    pub cc: CString
}

#[repr(C)]
pub struct InteropSortResult {
    pub count: usize,
    pub networks: *const InteropNetworkRange
}
