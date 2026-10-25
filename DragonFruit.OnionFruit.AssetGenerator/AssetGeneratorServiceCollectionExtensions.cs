// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using DragonFruit.OnionFruit.AssetGenerator.Abstractions;
using DragonFruit.OnionFruit.AssetGenerator.State;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DragonFruit.OnionFruit.AssetGenerator;

public static class AssetGeneratorServiceCollectionExtensions
{
    public static IServiceCollection AddAssetGeneration(this IServiceCollection services, Action<AssetGenerationOptions>? configure = null)
    {
        services.AddOptions<AssetGenerationOptions>();

        if (configure != null)
        {
            services.Configure(configure);
        }

        services.TryAddSingleton<IAssetGenerationStateStore, InMemoryAssetGenerationStateStore>();
        services.TryAddSingleton<AssetGenerationRunner>();

        return services;
    }
}
