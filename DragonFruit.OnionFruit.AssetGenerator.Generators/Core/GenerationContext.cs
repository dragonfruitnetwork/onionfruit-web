using System.Collections;
using System.Diagnostics.CodeAnalysis;
using DragonFruit.OnionFruit.AssetGenerator.Generators.Core.Abstractions;

namespace DragonFruit.OnionFruit.AssetGenerator.Generators.Core;

public sealed class GenerationContext : IReadOnlyDictionary<string, ISourceOutput>
{
    private readonly IReadOnlyDictionary<string, ISourceOutput> _outputs;

    public GenerationContext(IEnumerable<ISource> sources, IWorkspace workspace)
        : this(sources.SelectMany(source => source.Outputs), workspace)
    {
    }
    
    public GenerationContext(IEnumerable<KeyValuePair<string, ISourceOutput>> outputs, IWorkspace workspace)
    {
        _outputs = outputs.ToDictionary(kv => kv.Key, kv => kv.Value);
        Workspace = workspace;
    }

    public IWorkspace Workspace { get; }

    public bool HasOutput(string name) => _outputs.ContainsKey(name);

    public bool TryGetOutput<TOutput>(string name, [MaybeNullWhen(false)] out TOutput output) where TOutput : class, ISourceOutput
    {
        if (_outputs.TryGetValue(name, out var stored) && stored is TOutput typed)
        {
            output = typed;
            return true;
        }

        output = null;
        return false;
    }

    public TOutput GetOutput<TOutput>(string name) where TOutput : class, ISourceOutput
    {
        if (_outputs.TryGetValue(name, out var stored) && stored is TOutput typed)
        {
            return typed;
        }

        var state = stored is null
            ? "and no such output has been produced"
            : $"but an output of type '{stored.GetType().FullName}' is available";

        throw new KeyNotFoundException($"No output named '{name}' of type '{typeof(TOutput).FullName}' is available, {state}.");
    }

    public ISourceOutput this[string key] => _outputs[key];

    public IEnumerable<string> Keys => _outputs.Keys;

    public IEnumerable<ISourceOutput> Values => _outputs.Values;

    public int Count => _outputs.Count;

    public bool ContainsKey(string key) => _outputs.ContainsKey(key);

    public bool TryGetValue(string key, out ISourceOutput value) => _outputs.TryGetValue(key, out value!);

    public IEnumerator<KeyValuePair<string, ISourceOutput>> GetEnumerator() => _outputs.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
