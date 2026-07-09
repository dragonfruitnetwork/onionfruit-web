// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DragonFruit.OnionFruit.Web.Worker.Configuration;

public class RedisOptions : IValidatableObject
{
    public const string SectionName = "Redis";

    /// <summary>
    /// Full StackExchange.Redis connection string. If set, takes priority over the individual host/user/password properties.
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// Server hostname, used (along with <see cref="Port"/>, <see cref="User"/> and <see cref="Pass"/>) when <see cref="ConnectionString"/> is not set
    /// </summary>
    public string Host { get; set; } = "localhost";

    [Range(1, 65535)]
    public int Port { get; set; } = 6379;

    public string User { get; set; }
    public string Pass { get; set; }

    public bool Ssl { get; set; }
    public bool DisableCertValidation { get; set; }

    /// <summary>
    /// Prefix applied to all keys written to/read from the database
    /// </summary>
    [Required]
    public string KeyPrefix { get; set; } = "onionfruit-web-worker";

    /// <summary>
    /// Whether missing Redis.OM document indexes should be created at startup
    /// </summary>
    public bool CreateIndexes { get; set; } = true;

    /// <summary>
    /// Whether existing Redis.OM document indexes should be dropped and recreated at startup
    /// </summary>
    public bool RegenIndexes { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(ConnectionString) && string.IsNullOrEmpty(Host))
        {
            yield return new ValidationResult($"Either {nameof(ConnectionString)} or {nameof(Host)} must be set", [nameof(Host)]);
        }
    }
}
