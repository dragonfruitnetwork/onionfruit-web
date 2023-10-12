using System;
using System.Threading.Tasks;
using DragonFruit.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DragonFruit.OnionFruit.Web.Worker.Storage;

public class RemoteArchiveExporter : IDataExporter
{
    private const string DefaultPrefix = "onionfruit";
    private const string DatabaseSinkFileName = "{0}-data-{1}.zip";
    
    public string Prefix { get; set; }
    public string UploadUrl { get; set; }
    
    public async Task PerformUpload(IServiceProvider services, IUploadFileSource source)
    {
        await using var archiveStream = await source.CreateArchiveStream().ConfigureAwait(false);

        var logger = services.GetRequiredService<ILogger<RemoteArchiveExporter>>();
        var fileName = string.Format(DatabaseSinkFileName, Prefix ?? DefaultPrefix, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        
        logger.LogInformation("Uploading {name} ({x} bytes)", fileName, archiveStream.Length);

        // todo add retry policy to client
        var request = new GenericBlobUploadRequest(UploadUrl, fileName, archiveStream);
        using var response = await services.GetRequiredService<ApiClient>().PerformAsync(request).ConfigureAwait(false);

        logger.LogInformation("{file} uploaded with status code {code}", fileName, response.StatusCode);
    }

    public override string ToString() => $"Remote Archive Sink (prefix: {Prefix ?? "none"})";
}