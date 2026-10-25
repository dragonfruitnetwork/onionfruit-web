// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

namespace DragonFruit.OnionFruit.AssetGenerator;

public sealed class AssetGenerationOptions
{
    public string? WorkspaceRoot { get; set; }

    /// <summary>
    /// Names of generators that should not run.
    /// </summary>
    public HashSet<string> DisabledGenerators { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
