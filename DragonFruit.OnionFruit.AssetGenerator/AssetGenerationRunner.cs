// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Globalization;
using DragonFruit.OnionFruit.AssetGenerator.Abstractions;
using DragonFruit.OnionFruit.AssetGenerator.Generators.Core;
using DragonFruit.OnionFruit.AssetGenerator.Generators.Core.Abstractions;
using DragonFruit.OnionFruit.AssetGenerator.Workspace;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DragonFruit.OnionFruit.AssetGenerator;

public sealed class AssetGenerationRunner(
    IEnumerable<ISource> sources,
    IEnumerable<IGenerator> generators,
    IAssetGenerationStateStore stateStore,
    IEnumerable<IAssetPublisher> publishers,
    IOptions<AssetGenerationOptions> options,
    ILogger<AssetGenerationRunner> logger)
{
    public async Task<AssetGenerationResult> RunAsync(CancellationToken ct = default)
    {
        var runVersion = DateTimeOffset.UtcNow;
        var enabledGenerators = generators.Where(generator => !options.Value.DisabledGenerators.Contains(generator.Name)).ToList();
        var requiredOutputs = enabledGenerators.SelectMany(generator => generator.Sources).ToHashSet(StringComparer.Ordinal);
        var requiredSources = sources
            .Where(source => source.OutputNames.Any(requiredOutputs.Contains))
            .DistinctBy(source => source.Name)
            .ToList();

        if (enabledGenerators.Count == 0)
        {
            logger.LogInformation("No asset generators are enabled");
            return new AssetGenerationResult(runVersion, false, string.Empty, [], []);
        }

        var changedSources = new List<ISource>();

        foreach (var source in requiredSources)
        {
            ct.ThrowIfCancellationRequested();

            var lastRun = await stateStore.GetLastRunAsync(source.Name, ct) ?? DateTimeOffset.MinValue;

            logger.LogDebug("Checking whether {source} has changed since {lastRun}", source.Name, lastRun);

            if (await source.HasDataChangedAsync(lastRun, ct))
            {
                changedSources.Add(source);
            }
        }

        if (changedSources.Count == 0)
        {
            logger.LogInformation("No source data has changed; generator execution skipped");
            return new AssetGenerationResult(runVersion, false, string.Empty, [], []);
        }

        logger.LogInformation("Fetching {count} sources after detecting changes in {changed}", requiredSources.Count, string.Join(", ", changedSources.Select(source => source.Name)));
        await Task.WhenAll(requiredSources.Select(source => source.FetchAsync(ct)));

        var version = runVersion.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
        using var workspace = DirectoryWorkspace.CreateTemporary(options.Value.WorkspaceRoot, version);

        var context = new GenerationContext(requiredSources, workspace);

        var generatorResults = new List<GeneratorRunResult>(enabledGenerators.Count);
        var errors = new List<Exception>();

        foreach (var generator in enabledGenerators)
        {
            ct.ThrowIfCancellationRequested();

            var missingOutputs = generator.Sources.Where(output => !context.ContainsKey(output)).ToArray();
            if (missingOutputs.Length > 0)
            {
                var exception = new InvalidOperationException($"Generator '{generator.Name}' requires missing source output(s): {string.Join(", ", missingOutputs)}");

                logger.LogError(exception, "Skipping {generator}", generator.Name);
                generatorResults.Add(new GeneratorRunResult(generator.Name, false, exception));
                errors.Add(exception);
                continue;
            }

            try
            {
                logger.LogInformation("Running generator {generator}", generator.Name);
                await generator.GenerateAsync(context, ct);

                generatorResults.Add(new GeneratorRunResult(generator.Name, true, null));
                logger.LogInformation("Generator {generator} completed", generator.Name);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Generator {generator} failed", generator.Name);
                generatorResults.Add(new GeneratorRunResult(generator.Name, false, exception));
                errors.Add(exception);
            }
        }

        var assets = Directory.EnumerateFiles(workspace.RootPath, "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(workspace.RootPath, path).Replace(Path.DirectorySeparatorChar, '/'))
            .Order(StringComparer.Ordinal)
            .ToArray();

        if (errors.Count == 0)
        {
            var publishContext = new AssetPublishContext(workspace.RootPath, version, assets);

            foreach (var publisher in publishers)
            {
                logger.LogInformation("Publishing {count} assets with {publisher}", assets.Length, publisher.GetType().Name);
                await publisher.PublishAsync(publishContext, ct);
            }

            foreach (var source in requiredSources)
            {
                await stateStore.SetLastRunAsync(source.Name, runVersion, ct);
            }
        }
        else
        {
            logger.LogWarning("Generation completed with {count} error(s); assets were not published and the workspace was retained at {workspace}", errors.Count, workspace.RootPath);
        }

        return new AssetGenerationResult(runVersion, true, workspace.RootPath, generatorResults, errors);
    }
}
