using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DnsClient;
using DragonFruit.Data;
using libloc;
using libloc.Abstractions;
using libloc.Access;
using SharpCompress.Compressors.Xz;

namespace DragonFruit.OnionFruit.Web.Worker.Sources;

public class LocationDbSource : IDataSource, IDisposable
{
    private readonly ILookupClient _dnsClient;
    private readonly ApiClient _apiClient;

    private const string LastUpdateRecordAddress = "_v1._db.location.ipfire.org";

    public LocationDbSource(ILookupClient dnsClient, ApiClient apiClient)
    {
        _dnsClient = dnsClient;
        _apiClient = apiClient;
    }

    public ILocationDatabase Database { get; private set; }

    public async Task<bool> HasDataChanged(DateTimeOffset lastVersionDate)
    {
        var records = await _dnsClient.QueryAsync(LastUpdateRecordAddress, QueryType.TXT).ConfigureAwait(false);
        var date = records.Answers.SingleOrDefault()?.ToString();

        if (string.IsNullOrEmpty(date))
        {
            return true;
        }

        return DateTimeOffset.Parse(date, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind) > lastVersionDate;
    }

    public async Task CollectData()
    {
        var dbFileStream = new FileStream(Path.GetTempFileName(), FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan | FileOptions.DeleteOnClose);

        await using (var downloadStream = await _apiClient.PerformAsync<FileStream>(new LocationDbDownloadRequest()).ConfigureAwait(false))
        await using (var xzStream = new XZStream(downloadStream))
        {
            await xzStream.CopyToAsync(dbFileStream).ConfigureAwait(false);
        }

        Database = DatabaseLoader.LoadFromStream(dbFileStream);
    }

    public void Dispose()
    {
        Database?.Dispose();
    }
}