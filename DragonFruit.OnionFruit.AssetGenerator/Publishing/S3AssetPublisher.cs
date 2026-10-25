// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Security.Cryptography;
using Amazon.S3;
using Amazon.S3.Model;
using DragonFruit.OnionFruit.AssetGenerator.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DragonFruit.OnionFruit.AssetGenerator.Publishing;

/// <summary>
/// Uploads generated assets to S3 (or an S3-compatible endpoint).
/// </summary>
public sealed class S3AssetPublisher(IAmazonS3 client, IOptions<S3AssetPublisherOptions> options, ILogger<S3AssetPublisher> logger) : IAssetPublisher
{
    public async Task PublishAsync(AssetPublishContext context, CancellationToken ct)
    {
        foreach (var asset in context.Assets)
        {
            ct.ThrowIfCancellationRequested();

            var path = Path.Combine(context.WorkspaceRoot, asset.Replace('/', Path.DirectorySeparatorChar));

            if (!File.Exists(path))
            {
                continue;
            }

            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);

            var key = BuildKey(options.Value, context.Version, asset);
            var request = new PutObjectRequest
            {
                AutoCloseStream = false,
                BucketName = options.Value.BucketName,
                InputStream = stream,
                Key = key
            };

            if (options.Value.UseChecksum)
            {
                var checksum = await SHA256.HashDataAsync(stream, ct);
                stream.Seek(0, SeekOrigin.Begin);

                request.ChecksumAlgorithm = ChecksumAlgorithm.SHA256;
                request.ChecksumSHA256 = Convert.ToBase64String(checksum);
                request.DisablePayloadSigning = options.Value.Endpoint?.StartsWith("https://", StringComparison.OrdinalIgnoreCase) == true;
            }
            else
            {
                // third-party S3 implementations do not always understand the SDK's default checksum headers
                request.DisableDefaultChecksumValidation = true;
            }

            try
            {
                logger.LogInformation("Uploading asset {asset} to {bucket}/{key}", asset, options.Value.BucketName, key);
                await client.PutObjectAsync(request, ct);
                logger.LogInformation("Uploaded asset {asset}", asset);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to upload asset {asset} to {bucket}/{key}", asset, options.Value.BucketName, key);
                throw;
            }
        }
    }

    private static string BuildKey(S3AssetPublisherOptions options, string version, string asset)
    {
        var parts = new List<string>(3);

        if (!string.IsNullOrWhiteSpace(options.KeyPrefix))
        {
            parts.Add(options.KeyPrefix.Trim('/'));
        }

        parts.Add(version);
        parts.Add(asset.TrimStart('/'));

        return string.Join('/', parts);
    }
}
