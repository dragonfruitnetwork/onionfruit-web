// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using Coravel.Invocable;

namespace DragonFruit.OnionFruit.AssetGenerator.Worker.Services;

public sealed class AssetGenerationJob(AssetGenerationRunner runner, ILogger<AssetGenerationJob> logger) : IInvocable
{
    public async Task Invoke()
    {
        logger.LogInformation("Asset generation run requested");

        try
        {
            var result = await runner.RunAsync();

            switch (result.SkipReason)
            {
                case AssetGenerationSkipReason.NoSourceChanges:
                    logger.LogInformation("Asset generation run skipped: no data source changes detected");
                    return;

                case AssetGenerationSkipReason.NoGeneratorsEnabled:
                    logger.LogInformation("Asset generation run skipped: no asset generators are enabled");
                    return;
            }

            logger.LogInformation("Asset generation finished with {count} generator(s), {errors} error(s)", result.Generators.Count, result.Errors.Count);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Asset generation job failed");
            throw;
        }
    }
}