using System;
using System.Threading.Tasks;
using DragonFruit.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DragonFruit.OnionFruit.Web.Worker.Storage;

public class ArchiveExporter : IDataExporter
{
    private const string DefaultPrefix = "onionfruit";
    private const string DatabaseSinkFileName = "{0}-data-{1}.zip";
    
    private readonly string _prefix;
    private readonly string _uploadUrl;
    
    public ArchiveExporter(string prefix, string uploadUrl)
    {
        _prefix = prefix;
        _uploadUrl = uploadUrl;
    }
    
    public async Task PerformUpload(IServiceProvider services, IUploadFileSource source)
    {
        await using var archiveStream = await source.CreateArchiveStream().ConfigureAwait(false);

        var logger = services.GetRequiredService<ILogger<ArchiveExporter>>();
        var fileName = string.Format(DatabaseSinkFileName, _prefix ?? DefaultPrefix, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        
        logger.LogInformation("Uploading {name} ({x} bytes)", fileName, archiveStream.Length);

        // todo add retry policy to client
        var request = new GenericBlobUploadRequest(_uploadUrl, fileName, archiveStream);
        using var response = await services.GetRequiredService<ApiClient>().PerformAsync(request).ConfigureAwait(false);

        logger.LogInformation("{file} uploaded with status code {code}", fileName, response.StatusCode);
    }    
}