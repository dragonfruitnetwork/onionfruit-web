// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Net;
using DragonFruit.OnionFruit.Web.Models;

namespace DragonFruit.OnionFruit.Web.Services;

public interface IConnectionStatusService
{
    Task<ConnectionStatus> GetStatusAsync(IPAddress? address, string? cloudflareCountry, CancellationToken ct = default);
}
