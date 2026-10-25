// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

namespace DragonFruit.OnionFruit.AssetGenerator.Worker.Configuration;

public sealed class WorkerOptions
{
    public const string SectionName = "Worker";

    /// <summary>
    /// Cron expression used to schedule generation runs.
    /// </summary>
    public string CronSchedule { get; set; } = "0 */12 * * *";

    /// <summary>
    /// Whether a generation run should be queued immediately after startup.
    /// </summary>
    public bool RunOnStartup { get; set; } = true;

    /// <summary>
    /// Whether missing Redis.OM document indexes should be created at startup.
    /// </summary>
    public bool CreateIndexes { get; set; } = true;

    /// <summary>
    /// Whether existing Redis.OM document indexes should be dropped and recreated at startup.
    /// </summary>
    public bool RegenIndexes { get; set; }
}
