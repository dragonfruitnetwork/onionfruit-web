// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using DragonFruit.OnionFruit.AssetGenerator.Generators.Core.Abstractions;
using DragonFruit.OnionFruit.AssetGenerator.Generators.Legacy;
using DragonFruit.OnionFruit.AssetGenerator.Generators.OnionDb;
using DragonFruit.OnionFruit.AssetGenerator.Generators.RedisDb;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DragonFruit.OnionFruit.AssetGenerator.Generators;

public static class AssetGeneratorServiceCollectionExtensions
{
    public static IServiceCollection AddAssetGeneratorGenerators(this IServiceCollection services, bool includeRedis = true)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IGenerator, CsvAddressRangeListGenerator>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IGenerator, CountryListGenerator>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IGenerator, OnionDbGenerator>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IGenerator, OnionDbLiteGenerator>());

        if (includeRedis)
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IGenerator, RedisDatabaseGenerator>());
        }

        return services;
    }
}
