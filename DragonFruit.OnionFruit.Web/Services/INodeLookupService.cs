// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using DragonFruit.OnionFruit.Web.Shared.Redis.Nodes;

namespace DragonFruit.OnionFruit.Web.Services;

/// <summary>
/// Resolves OnionFruit node metadata for an IP address.
/// </summary>
public interface INodeLookupService
{
    Task<OnionFruitNodeInfo?> GetByIpAddressAsync(string ipAddress, CancellationToken ct = default);
}
