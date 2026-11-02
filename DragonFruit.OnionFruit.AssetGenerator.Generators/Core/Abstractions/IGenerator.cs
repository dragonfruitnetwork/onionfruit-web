// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

namespace DragonFruit.OnionFruit.AssetGenerator.Generators.Core.Abstractions;

public interface IGenerator
{
    string Name { get; }

    IReadOnlySet<string> Sources { get; }
    IReadOnlySet<string> Produces { get; }

    Task GenerateAsync(GenerationContext context, CancellationToken ct);
}