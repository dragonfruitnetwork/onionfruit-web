// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using DragonFruit.OnionFruit.Web.Worker;
using DragonFruit.OnionFruit.Web.Worker.Database;
using JetBrains.Annotations;
using libloc.Abstractions;
using libloc.Access;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Redis.OM.Contracts;

namespace DragonFruit.OnionFruit.Web.Controllers;

[UsedImplicitly]
public record ConnectionStatusResponse(string IpAddress, bool IsTor, string CountryCode, string CountryName, int? ASNumber, string ASName);

[EnableCors]
[Route("connectionStatus")]
public class ConnectionStatusController(IRedisConnectionProvider redis, ILocationDbAccessor libloc) : ControllerBase
{
    [HttpGet]
    public async Task<ConnectionStatusResponse> GetConnectionStatus()
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress;
        var connectionIpAddress = ipAddress?.AddressFamily switch
        {
            AddressFamily.InterNetwork => ipAddress.ToString(),
            AddressFamily.InterNetworkV6 => $"[{ipAddress}]",

            _ => null
        };

        if (connectionIpAddress == null)
        {
            return new ConnectionStatusResponse(string.Empty, false, "XX", "Unknown", 0, "Unknown AS");
        }

        // check internal redis database of known nodes
        var nodeInfo = await redis.RedisCollection<OnionFruitNodeInfo>().SingleOrDefaultAsync(x => x.IpAddress == connectionIpAddress).ConfigureAwait(false);
        if (nodeInfo != null)
        {
            return new ConnectionStatusResponse(ipAddress.ToString(), true, nodeInfo.CountryCode, nodeInfo.CountryName, nodeInfo.ProviderNumber, nodeInfo.ProviderName);
        }

        var cloudflareEnabled = HttpContext.Request.Headers.TryGetValue("CF-IPCountry", out var cloudflareCountry);
        var (countryCode, countryName, asInfo) = await libloc.PerformAsync(db => GetConnectionInfo(db, ipAddress, cloudflareEnabled ? cloudflareCountry.ToString() : null)).ConfigureAwait(false);

        return new ConnectionStatusResponse(ipAddress.ToString(), cloudflareEnabled && cloudflareCountry.ToString() is "T1" or "XX", countryCode, CountryMap.Instance.GetCountryName(countryCode) ?? countryName, asInfo?.Number, asInfo?.Name);
    }

    private static (string countryCode, string countryName, IDatabaseAS asInfo) GetConnectionInfo(ILocationDatabase x, IPAddress address, [CanBeNull] string countryCodeOverride)
    {
        IDatabaseAS asInfo = null;
        IDatabaseCountry country = null;

        var addr = x.ResolveAddress(address);

        if (addr != null)
        {
            asInfo = x.AS.GetAS((int)addr.ASN);
            country = x.Countries.GetCountry(countryCodeOverride ?? addr.CountryCode);
        }

        return (country?.Code ?? "XX", country?.Name ?? "Unknown Country", asInfo);
    }
}