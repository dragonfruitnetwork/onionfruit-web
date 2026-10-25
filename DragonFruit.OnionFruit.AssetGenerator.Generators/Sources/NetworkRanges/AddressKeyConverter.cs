using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;

namespace DragonFruit.OnionFruit.AssetGenerator.Generators.Sources.NetworkRanges;

internal static class AddressKeyConverter
{
    public static uint ToUInt32(IPAddress address)
    {
        var bytes = address.GetAddressBytes();
        if (bytes.Length == 16)
        {
            if (!address.IsIPv4MappedToIPv6)
            {
                throw new ArgumentException($"Address {address} is not an IPv4 or IPv4-mapped address.", nameof(address));
            }

            bytes = address.MapToIPv4().GetAddressBytes();
        }

        return BinaryPrimitives.ReadUInt32BigEndian(bytes);
    }

    public static UInt128 ToUInt128(IPAddress address)
    {
        var bytes = address.AddressFamily == AddressFamily.InterNetwork
            ? address.MapToIPv6().GetAddressBytes()
            : address.GetAddressBytes();

        return bytes.Aggregate<byte, UInt128>(0, (current, b) => (current << 8) | b);
    }
}
