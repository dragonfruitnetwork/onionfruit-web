// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Security.Cryptography;
using DragonFruit.OnionFruit.Redis.Assets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Redis.OM;
using Redis.OM.Contracts;
using StackExchange.Redis;

namespace DragonFruit.OnionFruit.Redis;

public static class RedisServiceCollectionExtensions
{
    /// <summary>
    /// Registers the shared Redis connection and the asset index used to resolve the latest version of an asset.
    /// </summary>
    public static IServiceCollection AddOnionFruitRedis(this IServiceCollection services)
    {
        services.AddOptions<RedisOptions>()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.TryAddSingleton<IConnectionMultiplexer>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<RedisOptions>>().Value;
            var connection = ConfigurationOptions.Parse(options.ConnectionString);

            if (!string.IsNullOrWhiteSpace(options.PinnedCertificateThumbprint))
            {
                var pinnedThumbprint = ParseThumbprint(options.PinnedCertificateThumbprint);

                connection.CertificateValidation += (_, certificate, _, _) =>
                {
                    Span<byte> thumbprint = stackalloc byte[20];

                    if (certificate is null || !certificate.TryGetCertHash(HashAlgorithmName.SHA1, thumbprint, out var written) || written != thumbprint.Length)
                    {
                        return false;
                    }

                    return CryptographicOperations.FixedTimeEquals(thumbprint, pinnedThumbprint);
                };
            }
            else if (options.DisableCertificateValidation)
            {
                connection.CertificateValidation += delegate { return true; };
            }

            return ConnectionMultiplexer.Connect(connection);
        });

        services.TryAddSingleton<IRedisConnectionProvider>(provider => new RedisConnectionProvider(provider.GetRequiredService<IConnectionMultiplexer>()));
        services.TryAddSingleton<IAssetIndex, RedisAssetIndex>();

        return services;
    }

    private static byte[] ParseThumbprint(string thumbprint)
    {
        var normalized = new string(thumbprint.Where(char.IsAsciiHexDigit).ToArray());
        if (normalized.Length != 40)
        {
            throw new InvalidOperationException($"The configured Redis certificate thumbprint '{thumbprint}' is not a valid SHA-1 thumbprint");
        }

        return Convert.FromHexString(normalized);
    }
}
