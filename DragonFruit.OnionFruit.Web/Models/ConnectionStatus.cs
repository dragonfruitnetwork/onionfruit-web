// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

namespace DragonFruit.OnionFruit.Web.Models;

public sealed record ConnectionStatus(
    string IpAddress,
    bool IsTor,
    string CountryCode,
    string CountryName,
    int? ProviderNumber,
    string? ProviderName);
