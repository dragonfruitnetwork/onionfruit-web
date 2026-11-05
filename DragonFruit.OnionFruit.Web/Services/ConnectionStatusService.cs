// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Net;
using System.Net.Sockets;
using DragonFruit.OnionFruit.Web.Models;
using DragonFruit.OnionFruit.Web.Shared;
using libloc.Abstractions;
using libloc.Access;

namespace DragonFruit.OnionFruit.Web.Services;

internal sealed class ConnectionStatusService(INodeLookupService nodeLookup, ILocationDbAccessor locationDb) : IConnectionStatusService
{
    private const string UnknownCountryCode = "XX";
    private const string UnknownCountryName = "Unknown";

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

        ConnectionStatus connectionStatus;

        var nodeInfo = await nodeLookup.GetByIpAddressAsync(connectionIpAddress, ct);
        if (nodeInfo is not null)
        {
            var countryCode = nodeInfo.CountryCode ?? UnknownCountryCode;
            var countryName = CountryNames.CountryNameMapping.GetValueOrDefault(countryCode, nodeInfo.CountryName ?? UnknownCountryName);

            connectionStatus = new ConnectionStatus(
                address.ToString(),
                true,
                countryCode,
                countryName,
                nodeInfo.ProviderNumber,
                nodeInfo.ProviderName);
        }
        else
        {
            var (countryCode, countryName, asInfo) = await locationDb.PerformAsync(database => GetConnectionInfo(database, address, cloudflareCountry));

            connectionStatus = new ConnectionStatus(
                address.ToString(),
                cloudflareCountry is "T1",
                countryCode,
                CountryNames.CountryNameMapping.GetValueOrDefault(countryCode, countryName),
                asInfo?.Number,
                asInfo?.Name);
        }

        return connectionStatus;
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

    private static ConnectionStatus Unknown()
    {
        return new ConnectionStatus(string.Empty, false, UnknownCountryCode, UnknownCountryName, null, null);
    }
}