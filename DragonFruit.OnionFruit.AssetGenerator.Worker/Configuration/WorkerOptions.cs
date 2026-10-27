// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

namespace DragonFruit.OnionFruit.AssetGenerator.Worker.Configuration;

public sealed class WorkerOptions
{
    public const string SectionName = "Worker";

    /// <summary>
    /// Whether a generation run should be queued immediately after startup.
    /// </summary>
    public bool RunOnStartup { get; set; } = true;

    /// <summary>
    /// Whether the expired-asset purge job should be run
    /// </summary>
    public bool PurgeExpiredAssets { get; set; } = true;

    /// <summary>
    /// Whether missing Redis document indexes should be created at startup.
    /// </summary>
    public bool CreateIndexes { get; set; } = true;

    /// <summary>
    /// Whether existing Redis document indexes should be dropped and recreated at startup.
    /// </summary>
    public bool RegenIndexes { get; set; }
}