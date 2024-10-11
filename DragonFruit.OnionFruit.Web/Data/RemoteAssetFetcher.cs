// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using DragonFruit.Data;
using Json.Path;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace DragonFruit.OnionFruit.Web.Data
{
    public class RemoteAssetFetcher : IHostedService
    {
        private readonly ILogger<RemoteAssetFetcher> _logger;
        private readonly IConnectionMultiplexer _redis;
        private readonly IConfiguration _configuration;
        private readonly JsonPath _assetSelectorPath;
        private readonly LocalAssetStore _assetStore;
        private readonly ApiClient _client;

        private ChannelMessageQueue _redisMessageQueue;
        private Timer _timer;

        public RemoteAssetFetcher(ILogger<RemoteAssetFetcher> logger, IConnectionMultiplexer redis, IConfiguration configuration, LocalAssetStore assetStore, ApiClient client)
        {
            _logger = logger;
            _redis = redis;
            _configuration = configuration;
            _assetStore = assetStore;
            _client = client;

            var jsonPath = _configuration["Server:RemoteAssets:ListingPath"] ?? "$[*]";
            if (!JsonPath.TryParse(jsonPath, out _assetSelectorPath))
            {
                _logger.LogError("Path expression {p} was invalid. Execution cannot continue until fixed", jsonPath);
            }
        }

        private async Task PerformAssetCheck()
        {
            _logger.LogInformation("Performing asset download check");

            PathResult eval;

            var clock = new Stopwatch();
            var listingUrl = _configuration["Server:RemoteAssets:ListingUrl"] ?? string.Format(_configuration["Server:RemoteAssets:DownloadUrl"], string.Empty);

            clock.Start();

            try
            {
                var jsonNode = await _client.PerformAsync<JsonObject>(listingUrl).ConfigureAwait(false);
                eval = _assetSelectorPath.Evaluate(jsonNode);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Asset listing request failed: {err}", e.Message);
                throw;
            }

            if (!eval.Matches.Any())
            {
                _logger.LogError("Path {p} could not be evaluated", _assetSelectorPath);
            }

            // convert json elements to objects, filter out non .zip files and get newest one
            var targetElement = eval.Matches.Where(x => x.Value != null)
                .Select(x => new
                {
                    Name = x.Value[_configuration["Server:RemoteAssets:EntryNameKey"]]?.GetValue<string>(),
                    Date = x.Value[_configuration["Server:RemoteAssets:EntryDateKey"]]?.GetValue<DateTime>()
                })
                .Where(x => Path.GetExtension(x.Name).Equals(".zip", StringComparison.InvariantCultureIgnoreCase))
                .MaxBy(x => x.Date);

            if (targetElement == null)
            {
                _logger.LogInformation("No elements were found. Skipping...");
                return;
            }

            _logger.LogInformation("Latest uploaded dataset is {name} ({date})", targetElement.Name, targetElement.Date);
            var revisionId = FileNameToRevisionId(targetElement.Name);

            // use check to determine if the version already exists and has files inside
            if (_assetStore.AssetRevisionExists(revisionId))
            {
                _logger.LogInformation("Version already exists locally. Skipping download...");
                return;
            }

            await DownloadAssetBundle(targetElement.Name, revisionId).ConfigureAwait(false);

            clock.Stop();
            _logger.LogInformation("Asset check completed successfully ({x} elapsed)", clock.Elapsed);
        }

        private async Task DownloadAssetBundle(string name, string revisionId = null)
        {
            if (!Path.GetExtension(name).Equals(".zip", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("File {name} is not a zip file and cannot be used.", name);
                return;
            }

            var downloadUrl = string.Format(_configuration["Server:RemoteAssets:DownloadUrl"], name);

            using var content = await _client.PerformAsync<FileStream>(downloadUrl).ConfigureAwait(false);
            using var zipStream = new ZipArchive(content, ZipArchiveMode.Read);

            var assetStore = _assetStore.CreateNewAssetStoreRevision(revisionId ??= FileNameToRevisionId(name));

            foreach (var zipEntry in zipStream.Entries)
            {
                using var zipEntryStream = zipEntry.Open();
                await assetStore.AddFile(zipEntry.FullName, zipEntryStream).ConfigureAwait(false);
            }

            _logger.LogInformation("Asset store revision {id} submitted successfully.", revisionId);
        }

        /// <summary>
        /// Event callback triggered by the local <see cref="_timer"/>
        /// </summary>
        private async Task PerformTimerUpdate()
        {
            _logger.LogInformation("Performing Timer-based asset update check...");

            try
            {
                await PerformAssetCheck().ConfigureAwait(false);
                _timer.Change(TimeSpan.FromDays(1), Timeout.InfiniteTimeSpan);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Timer-controlled update failed: {err}", e.Message);
                _timer.Change(TimeSpan.FromMinutes(30), Timeout.InfiniteTimeSpan);
            }
        }

        /// <summary>
        /// Event callback triggered by the attached redis database.
        /// </summary>
        private async Task PerformRedisUpdate(ChannelMessage channel)
        {
            if (channel.Message.IsNullOrEmpty)
            {
                return;
            }

            try
            {
                await DownloadAssetBundle(channel.Message).ConfigureAwait(false);
                _timer?.Change(TimeSpan.FromDays(1), Timeout.InfiniteTimeSpan);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Redis-based update (asset id {id}) failed: {err}", channel.Message, e.Message);
            }
        }

        private static string FileNameToRevisionId(string fileName)
        {
            var normalisedFileName = Path.GetFileNameWithoutExtension(fileName).ToLowerInvariant();
            var fileNameHash = SHA256.HashData(Encoding.UTF8.GetBytes(normalisedFileName));

            return Convert.ToHexString(fileNameHash).ToLowerInvariant();
        }

        async Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            // don't start timers if the path is invalid
            if (_assetSelectorPath == null)
            {
                return;
            }

            var channelName = _configuration["Server:RedisUpdateChannelName"];

            if (!string.IsNullOrEmpty(channelName))
            {
                var channelIdentifier = RedisChannel.Literal(channelName);

                _redisMessageQueue = await _redis.GetSubscriber().SubscribeAsync(channelIdentifier).ConfigureAwait(false);
                _redisMessageQueue.OnMessage(PerformRedisUpdate);
            }

            _timer = new Timer(_ => PerformTimerUpdate(), null, TimeSpan.Zero, Timeout.InfiniteTimeSpan);
        }

        async Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            if (_timer != null)
            {
                await _timer.DisposeAsync().ConfigureAwait(false);
            }

            if (_redisMessageQueue != null)
            {
                await _redisMessageQueue.UnsubscribeAsync();
            }
        }
    }
}