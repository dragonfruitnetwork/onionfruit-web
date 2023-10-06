/// sort first from largest to smallest, then by address.
impl Ord for super::NetBlock {
    fn cmp(&self, other: &Self) -> std::cmp::Ordering {
        self.network
            .prefix()
            .cmp(&other.network.prefix())
            .then_with(|| self.network.network().cmp(&other.network.network()))
    }
}

impl PartialOrd for super::NetBlock {
    fn partial_cmp(&self, other: &Self) -> Option<std::cmp::Ordering> {
        Some(self.cmp(other))
    }
}

impl PartialEq for super::NetBlock {
    fn eq(&self, other: &Self) -> bool {
        self.network == other.network
    }
}

impl Eq for super::NetBlock {}