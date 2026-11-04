// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using DnsClient;
using DragonFruit.Data;
using DragonFruit.Data.Serializers;
using DragonFruit.OnionFruit.AssetGenerator.Generators.Core.Abstractions;
using DragonFruit.OnionFruit.AssetGenerator.Generators.Sources.NetworkRanges;
using DragonFruit.OnionFruit.AssetGenerator.Generators.Sources.RelayListing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DragonFruit.OnionFruit.AssetGenerator.Generators.Sources;

public static class AssetGeneratorServiceCollectionExtensions
{
    public static IServiceCollection AddAssetGeneratorSources(this IServiceCollection services)
    {
        services.TryAddSingleton<ApiClient, ApiClient<ApiJsonSerializer>>();
        services.TryAddSingleton<ILookupClient, LookupClient>();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISource, IPFireNetworkRangeMapSource>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISource, OnionooSource>());

        return services;
    }
}