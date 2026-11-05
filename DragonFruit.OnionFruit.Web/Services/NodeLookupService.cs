// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using DragonFruit.OnionFruit.Web.Shared.Redis.Nodes;
using Polly;
using Polly.Retry;
using Redis.OM.Contracts;
using StackExchange.Redis;

namespace DragonFruit.OnionFruit.Web.Services;

internal sealed class NodeLookupService(IRedisConnectionProvider redis, ILogger<NodeLookupService> logger) : INodeLookupService
{
    private readonly ResiliencePipeline _retryPipeline = new ResiliencePipelineBuilder()
        .AddRetry(new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<RedisConnectionException>().Handle<RedisTimeoutException>(),
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromMilliseconds(250),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            OnRetry = args =>
            {
                logger.LogWarning(args.Outcome.Exception, "Redis node lookup failed on attempt {Attempt}; retrying in {Delay}", args.AttemptNumber, args.RetryDelay);
                return ValueTask.CompletedTask;
            }
        })
        .Build();

    public async Task<OnionFruitNodeInfo?> GetByIpAddressAsync(string ipAddress, CancellationToken ct = default)
    {
        return await _retryPipeline.ExecuteAsync(token => new ValueTask<OnionFruitNodeInfo?>(redis.RedisCollection<OnionFruitNodeInfo>().SingleOrDefaultAsync(x => x.IpAddress == ipAddress).WaitAsync(token)), ct);
    }
}
