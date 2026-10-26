// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Globalization;
using System.Net;
using System.Net.Sockets;
using DragonFruit.OnionFruit.Redis.Nodes;
using DragonFruit.OnionFruit.Web.Models;
using libloc.Abstractions;
using libloc.Access;
using Redis.OM.Contracts;

namespace DragonFruit.OnionFruit.Web.Services;

internal sealed class ConnectionStatusService(IRedisConnectionProvider redis, ILocationDbAccessor locationDb) : IConnectionStatusService
{
    private const string UnknownCountryCode = "XX";
    private const string UnknownCountryName = "Unknown Country";
    private const string UnknownProviderName = "Unknown AS";

    public async Task<ConnectionStatus> GetStatusAsync(IPAddress? address, string? cloudflareCountry, CancellationToken ct = default)
    {
        if (address is null)
        {
            return Unknown();
        }

        var connectionIpAddress = address.AddressFamily switch
        {
            AddressFamily.InterNetwork => address.ToString(),
            AddressFamily.InterNetworkV6 => $"[{address}]",

            _ => null
        };

        if (connectionIpAddress is null)
        {
            return Unknown();
        }

        ct.ThrowIfCancellationRequested();

        // check the internal Redis database of known Tor nodes first
        var nodeInfo = await redis.RedisCollection<OnionFruitNodeInfo>().SingleOrDefaultAsync(x => x.IpAddress == connectionIpAddress);

        if (nodeInfo is not null)
        {
            return new ConnectionStatus(
                address.ToString(),
                true,
                nodeInfo.CountryCode ?? UnknownCountryCode,
                nodeInfo.CountryName ?? "Unknown",
                nodeInfo.ProviderNumber,
                nodeInfo.ProviderName);
        }

        var cloudflareEnabled = cloudflareCountry is not null;
        var (countryCode, countryName, asInfo) = await locationDb.PerformAsync(database => GetConnectionInfo(database, address, cloudflareCountry));

        return new ConnectionStatus(
            address.ToString(),
            cloudflareEnabled && cloudflareCountry is "T1" or "XX",
            countryCode,
            GetCountryName(countryCode) ?? countryName,
            asInfo?.Number,
            asInfo?.Name);
    }

    private static (string CountryCode, string CountryName, IDatabaseAS? AsInfo) GetConnectionInfo(ILocationDatabase database, IPAddress address, string? countryCodeOverride)
    {
        IDatabaseAS? asInfo = null;
        IDatabaseCountry? country = null;

        var network = database.ResolveAddress(address);
        if (network is not null)
        {
            if (network.ASN > 0)
            {
                asInfo = database.AS.GetAS((int)network.ASN);
            }

            var countryCode = countryCodeOverride ?? network.CountryCode;
            if (!string.IsNullOrWhiteSpace(countryCode))
            {
                country = database.Countries.GetCountry(countryCode);
            }
        }

        return (country?.Code ?? UnknownCountryCode, country?.Name ?? UnknownCountryName, asInfo);
    }

    private static string? GetCountryName(string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode) || countryCode.Length != 2)
        {
            return null;
        }

        try
        {
            return new RegionInfo(countryCode.ToUpperInvariant()).EnglishName;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private static ConnectionStatus Unknown()
    {
        return new ConnectionStatus(string.Empty, false, UnknownCountryCode, "Unknown", 0, UnknownProviderName);
    }
}
