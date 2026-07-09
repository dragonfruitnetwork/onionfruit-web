// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DragonFruit.OnionFruit.Web.Worker.Configuration;

public static class ConfigurationExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers <typeparamref name="TOptions"/> bound to the named configuration section, with data annotation validation performed at startup.
        /// </summary>
        public OptionsBuilder<TOptions> AddValidatedOptions<TOptions>(IConfiguration configuration, string sectionName) where TOptions : class
            => services.AddOptions<TOptions>()
                .Bind(configuration.GetSection(sectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

        /// <summary>
        /// Registers <see cref="RedisOptions"/>, additionally applying the legacy top-level index management switches (REDIS_CREATE_INDEXES/REDIS_REGEN_INDEXES).
        /// </summary>
        public OptionsBuilder<RedisOptions> AddRedisOptions(IConfiguration configuration)
            => services.AddValidatedOptions<RedisOptions>(configuration, RedisOptions.SectionName)
                .PostConfigure(options =>
                {
                    if (configuration["REDIS_CREATE_INDEXES"] is { } createIndexes)
                    {
                        options.CreateIndexes = createIndexes.Equals("true", StringComparison.OrdinalIgnoreCase);
                    }

                    if (configuration["REDIS_REGEN_INDEXES"] is { } regenIndexes)
                    {
                        options.RegenIndexes = regenIndexes.Equals("true", StringComparison.OrdinalIgnoreCase);
                    }
                });
    }
}
