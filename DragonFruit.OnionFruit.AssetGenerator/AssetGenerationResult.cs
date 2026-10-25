// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

namespace DragonFruit.OnionFruit.AssetGenerator;

public enum AssetGenerationSkipReason
{
    None,
    NoGeneratorsEnabled,
    NoSourceChanges
}

public sealed record AssetGenerationResult(
    DateTimeOffset Version,
    bool SourcesChanged,
    string WorkspaceRoot,
    IReadOnlyList<GeneratorRunResult> Generators,
    IReadOnlyList<Exception> Errors,
    AssetGenerationSkipReason SkipReason = AssetGenerationSkipReason.None)
{
    public bool Succeeded => Errors.Count == 0;
}

public sealed record GeneratorRunResult(string Name, bool Succeeded, Exception? Error);
