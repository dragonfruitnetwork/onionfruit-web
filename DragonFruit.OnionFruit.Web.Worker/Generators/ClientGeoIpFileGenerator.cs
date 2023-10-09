using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DragonFruit.OnionFruit.Web.Worker.Sources;
using DragonFruit.OnionFruit.Web.Worker.Storage;

namespace DragonFruit.OnionFruit.Web.Worker.Generators;

/// <summary>
/// Generator responsible for generating the CSV files used by Tor clients
/// </summary>
public class ClientGeoIpFileGenerator : IDatabaseGenerator
{
    private readonly LocationDbSource _locationDbSource;

    public ClientGeoIpFileGenerator(LocationDbSource locationDbSource)
    {
        _locationDbSource = locationDbSource;
    }

    public async Task GenerateDatabase(IFileSink fileSink)
    {
        await using (var ip4FileStream = new StreamWriter(fileSink.CreateFile("legacy/geoip"), Encoding.ASCII, leaveOpen: true))
        {
            await WriteHeaderAsync(ip4FileStream, 4).ConfigureAwait(false);
            foreach (var entry in _locationDbSource.IPv4AddressRanges)
            {
                if (entry.CountryCode.Any(x => x is < 'A' or > 'Z'))
                {
                    continue;
                }

#pragma warning disable CS0618 // Type or member is obsolete
                var startAddr = (uint)IPAddress.HostToNetworkOrder((int)entry.Network.Begin.Address);
                var endAddr = (uint)IPAddress.HostToNetworkOrder((int)entry.Network.End.Address);
#pragma warning restore CS0618 // Type or member is obsolete

                await ip4FileStream.WriteLineAsync($"{startAddr},{endAddr},{entry.CountryCode}").ConfigureAwait(false);
            }
        }

        await using (var ip6FileStream = new StreamWriter(fileSink.CreateFile("legacy/geoip6"), Encoding.ASCII, leaveOpen: true))
        {
            await WriteHeaderAsync(ip6FileStream, 6).ConfigureAwait(false);
            foreach (var entry in _locationDbSource.IPv6AddressRanges)
            {
                if (entry.CountryCode.Any(x => x is < 'A' or > 'Z'))
                {
                    continue;
                }

                await ip6FileStream.WriteLineAsync($"{entry.Network.Begin},{entry.Network.End},{entry.CountryCode}").ConfigureAwait(false);
            }
        }
    }

    private async Task WriteHeaderAsync(TextWriter writer, int addressVersion)
    {
        await writer.WriteLineAsync($"# OnionFruit GeoIP File (IPv{addressVersion}). Generated using IPFire's location.db licenced under {_locationDbSource.Database.License}.").ConfigureAwait(false);
    }
}