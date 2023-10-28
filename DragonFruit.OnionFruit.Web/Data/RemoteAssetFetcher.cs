// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
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
        private readonly LocalAssetStore _assetStore;
        private readonly ApiClient _client;

        private Timer _timer;

        public RemoteAssetFetcher(ILogger<RemoteAssetFetcher> logger, IConnectionMultiplexer redis, IConfiguration configuration, LocalAssetStore assetStore, ApiClient client)
        {
            _logger = logger;
            _redis = redis;
            _configuration = configuration;
            _assetStore = assetStore;
            _client = client;
        }

        private async Task PerformAssetCheck()
        {
            _logger.LogInformation("Performing asset download check");

            var listingUrl = _configuration["RemoteAssets:ListingUrl"] ?? string.Format(_configuration["RemoteAssets:DownloadUrl"], string.Empty);
            var jsonPath = _configuration["RemoteAssets:ListingPath"] ?? "$[*]";

            if (!JsonPath.TryParse(jsonPath, out var jsonPathSelector))
            {
                _logger.LogError("Path expression {p} was invalid. Execution cannot continue until fixed", jsonPath);
                return;
            }

            PathResult eval;

            try
            {
                var jsonNode = await _client.PerformAsync<JsonNode>(listingUrl).ConfigureAwait(false);
                eval = jsonPathSelector.Evaluate(jsonNode);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Asset listing request failed: {err}", e.Message);
                throw;
            }

            if (!string.IsNullOrEmpty(eval.Error))
            {
                _logger.LogError("Path {p} could not be evaluated: {err}", jsonPath, eval.Error);
            }

            // convert json elements to objects, filter out non .zip files and get newest one
            var targetElement = eval.Matches.Where(x => x.Value != null)
                .Select(x => new
                {
                    Name = x.Value[_configuration["RemoteAssets:EntryNameKey"]]?.GetValue<string>(),
                    Date = x.Value[_configuration["RemoteAssets:EntryDateKey"]]?.GetValue<DateTime>()
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
        }

        private async Task DownloadAssetBundle(string name, string revisionId = null)
        {
            var downloadUrl = string.Format(_configuration["RemoteAssets:DownloadUrl"], name);

            using var content = await _client.PerformAsync<FileStream>(downloadUrl);
            using var zipStream = new ZipArchive(content, ZipArchiveMode.Read);

            var assetStore = _assetStore.CreateNewAssetStoreRevision(revisionId ?? FileNameToRevisionId(name));

            foreach (var zipEntry in zipStream.Entries)
            {
                using var zipEntryStream = zipEntry.Open();
                await assetStore.AddFile(zipEntry.FullName, zipEntryStream).ConfigureAwait(false);
            }
        }

        private static string FileNameToRevisionId(string fileName)
        {
            var normalisedFileName = Path.GetFileNameWithoutExtension(fileName).ToLowerInvariant();
            var fileNameHash = SHA256.HashData(Encoding.UTF8.GetBytes(normalisedFileName));

            return Convert.ToHexString(fileNameHash).ToLowerInvariant();
        }

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}