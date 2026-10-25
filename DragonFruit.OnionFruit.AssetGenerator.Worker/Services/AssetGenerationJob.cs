// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using Coravel.Invocable;

namespace DragonFruit.OnionFruit.AssetGenerator.Worker.Services;

public sealed class AssetGenerationJob(AssetGenerationRunner runner, ILogger<AssetGenerationJob> logger) : IInvocable
{
    public async Task Invoke()
    {
        try
        {
            var result = await runner.RunAsync();

            if (result.SourcesChanged)
            {
                logger.LogInformation("Asset generation finished with {count} generator(s), {errors} error(s)", result.Generators.Count, result.Errors.Count);
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Asset generation job failed");
            throw;
        }
    }
}
