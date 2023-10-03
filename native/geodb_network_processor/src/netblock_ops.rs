/// sort first from largest to smallest, then by address.
impl Ord for super::NetBlock {
    fn cmp(&self, other: &Self) -> std::cmp::Ordering {
        self.net
            .prefix()
            .cmp(&other.net.prefix())
            .then_with(|| self.net.network().cmp(&other.net.network()))
    }
}

impl PartialOrd for super::NetBlock {
    fn partial_cmp(&self, other: &Self) -> Option<std::cmp::Ordering> {
        Some(self.cmp(other))
    }
}

impl PartialEq for super::NetBlock {
    fn eq(&self, other: &Self) -> bool {
        self.net == other.net
    }
}

impl Eq for super::NetBlock {}