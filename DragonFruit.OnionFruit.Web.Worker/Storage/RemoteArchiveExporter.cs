// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DragonFruit.Data;
using DragonFruit.OnionFruit.Web.Worker.Storage.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace DragonFruit.OnionFruit.Web.Worker.Storage;

public class RemoteArchiveExporter : IDataExporter
{
    private const string DefaultPrefix = "onionfruit-data";
    private const string DatabaseSinkFileName = "{0}-{1}.zip";

    /// <summary>
    /// Optional prefix to add to the file before appending the current UNIX epoch.
    /// </summary>
    public string Prefix { get; set; }

    /// <summary>
    /// The URL to send a PUT request with the file's contents to.
    /// This url will be formatted to append the filename when the request is made.
    /// </summary>
    public string UploadUrl { get; set; }

    /// <summary>
    /// When set and the upload completes, a PubSub message will be sent to the specified channel with the filename of the newly uploaded file.
    /// </summary>
    [MaybeNull]
    public string RedisNotificationChannel { get; set; }

    public async Task PerformUpload(IServiceProvider services, IUploadFileSource source)
    {
        await using var archiveStream = await source.CreateArchiveStreamAsync().ConfigureAwait(false);

        var logger = services.GetRequiredService<ILogger<RemoteArchiveExporter>>();
        var fileName = string.Format(DatabaseSinkFileName, Prefix ?? DefaultPrefix, DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        var checksum = await MD5.HashDataAsync(archiveStream).ConfigureAwait(false);
        var request = new GenericBlobUploadRequest(UploadUrl, fileName, archiveStream, checksum);

        logger.LogInformation("Uploading {name} ({x} bytes)", fileName, archiveStream.Length);
        using var response = await services.GetRequiredService<ApiClient>().PerformAsync(request).ConfigureAwait(false);

        logger.LogInformation("{file} uploaded with status code {code}", fileName, response.StatusCode);

        if (response.IsSuccessStatusCode && !string.IsNullOrEmpty(RedisNotificationChannel))
        {
            var redis = services.GetRequiredService<IConnectionMultiplexer>().GetSubscriber();
            var messageHitCount = await redis.PublishAsync(RedisNotificationChannel, fileName).ConfigureAwait(false);

            logger.LogInformation("Upload notification successfully hit {x} clients", messageHitCount);
        }
    }

    public override string ToString() => $"Remote Archive Sink (prefix: {Prefix ?? "none"})";
}