// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DragonFruit.OnionFruit.Web.Worker.Sources;
using DragonFruit.OnionFruit.Web.Worker.Storage.Abstractions;

namespace DragonFruit.OnionFruit.Web.Worker.Generators;

/// <summary>
/// Generator responsible for generating the CSV files used by Tor clients
/// </summary>
public class ClientGeoIpFileGenerator(LocationDbSource locationDbSource) : IDatabaseGenerator
{
    public async Task GenerateDatabase(IFileSink fileSink)
    {
        await using (var ip4FileStream = new StreamWriter(fileSink.CreateFile("legacy/geoip"), Encoding.ASCII, leaveOpen: true))
        {
            await WriteHeaderAsync(ip4FileStream, 4).ConfigureAwait(false);
            foreach (var entry in locationDbSource.IPv4AddressRanges)
            {
                if (entry.CountryCode.Any(x => !char.IsAsciiLetterUpper(x)))
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
            foreach (var entry in locationDbSource.IPv6AddressRanges)
            {
                if (entry.CountryCode.Any(x => !char.IsAsciiLetterUpper(x)))
                {
                    continue;
                }

                await ip6FileStream.WriteLineAsync($"{entry.Network.Begin},{entry.Network.End},{entry.CountryCode}").ConfigureAwait(false);
            }
        }
    }

    private async Task WriteHeaderAsync(TextWriter writer, int addressVersion)
    {
        await writer.WriteLineAsync($"# OnionFruit GeoIP File (IPv{addressVersion}). Generated using IPFire's location.db licenced under {locationDbSource.Database.License}.").ConfigureAwait(false);
    }
}