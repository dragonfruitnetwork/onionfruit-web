namespace DragonFruit.OnionFruit.AssetGenerator.Generators.Core.Abstractions;

public interface IGenerator
{
    string Name { get; }

    IReadOnlySet<string> Sources { get; }
    IReadOnlySet<string> Produces { get; }

    Task GenerateAsync(GenerationContext context, CancellationToken ct);
}
