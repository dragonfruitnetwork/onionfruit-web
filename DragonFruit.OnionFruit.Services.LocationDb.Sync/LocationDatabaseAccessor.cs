// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DragonFruit.Data;
using DragonFruit.OnionFruit.Services.LocationDb.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using SharpCompress.Compressors.Xz;

namespace DragonFruit.OnionFruit.Services.LocationDb.Sync
{
    internal class LocationDatabaseAccessor : IDatabaseAccessor, IHostedService, IDisposable
    {
        private readonly ILogger<LocationDatabaseAccessor> _logger;
        private readonly IConfiguration _configuration;
        private readonly AsyncReaderWriterLock _lock;
        private readonly IDisposable _initialLock;
        private readonly ApiClient _client;

        private ILocationDatabase _database;
        private Timer _fetchTimer;

        public LocationDatabaseAccessor(ILogger<LocationDatabaseAccessor> logger, IConfiguration configuration, ApiClient client)
        {
            _logger = logger;
            _client = client;
            _configuration = configuration;
            _lock = new AsyncReaderWriterLock();

            _initialLock = _lock.WriterLock();
        }

        private const string DatabaseName = "location-{0}.db";

        private string DatabaseStorageRoot => _configuration["LocationDb:StorageRoot"];

        public async Task<T> PerformAsync<T>(Func<ILocationDatabase, T> action)
        {
            using (await _lock.ReaderLockAsync().ConfigureAwait(false))
            {
                return action.Invoke(_database);
            }
        }

        private async Task<bool> DownloadLatestDatabase()
        {
            var downloadRequest = new LocationDbDownloadRequest
            {
                LastDownload = _database?.CreatedAt
            };

            using var response = await _client.PerformAsync(downloadRequest).ConfigureAwait(false);

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                {
                    var version = (ulong)(response.Content.Headers.LastModified ?? DateTime.UtcNow).Subtract(DateTime.UnixEpoch).TotalSeconds;

                    _logger.LogInformation("New location.db discovered, writing version {ver} to disk", version);
                    await using var destinationStream = new FileStream(string.Format(DatabaseName, version), FileMode.Create, FileAccess.Write, FileShare.None, 8192, FileOptions.Asynchronous);

                    try
                    {
                        await using var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                        await using var xzStream = new XZStream(content);

                        var stopwatch = new Stopwatch();

                        stopwatch.Start();

                        // copy deflated stream to destination file
                        await xzStream.CopyToAsync(destinationStream).ConfigureAwait(false);

                        stopwatch.Stop();
                        _logger.LogInformation("location.db written to disk in {x} seconds", stopwatch.Elapsed.TotalSeconds.ToString("0.###"));
                    }
                    catch (Exception e)
                    {
                        await destinationStream.DisposeAsync().ConfigureAwait(false);
                        File.Delete(destinationStream.Name);

                        _logger.LogWarning("location.db download failed - {message}", e.Message);

                        return false;
                    }

                    return true;
                }

                case HttpStatusCode.NotModified:
                    _logger.LogInformation("location.db not modified since {date}, check completed successfully", response.Content.Headers.LastModified?.ToString("f"));
                    return false;

                default:
                    _logger.LogWarning("location.db check returned an unexpected result - {code}", response.StatusCode);
                    _logger.LogDebug("location.db check response - {content}", await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    return false;
            }
        }

        private async ValueTask<bool> LoadLatestDatabase(bool enforceLock = true)
        {
            // load in latest database from directory
            var dbFiles = Directory.GetFiles(DatabaseStorageRoot, string.Format(DatabaseName, "*"), SearchOption.TopDirectoryOnly);

            using (enforceLock ? await _lock.WriterLockAsync().ConfigureAwait(false) : null)
            {
                foreach (var dbFile in dbFiles.OrderByDescending(File.GetLastWriteTimeUtc))
                {
                    try
                    {
                        var newDatabase = DatabaseLoader.LoadFromFile(dbFile);

                        _database?.Dispose();
                        _database = newDatabase;

                        // remove all other location.db files
                        foreach (var file in dbFiles.Where(x => x != dbFile))
                        {
                            _logger.LogDebug("Removing old location db {name}", Path.GetFileName(file));
                            File.Delete(file);
                        }

                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("{db} failed to load - {message}", Path.GetFileName(dbFile), ex.Message);
                    }
                }
            }

            return false;
        }

        private async Task PerformUpdate()
        {
            if (await DownloadLatestDatabase().ConfigureAwait(false))
            {
                await LoadLatestDatabase().ConfigureAwait(false);
            }
        }

        async Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            using (_initialLock)
            {
                // load from disk
                var dbLoaded = await LoadLatestDatabase(false).ConfigureAwait(false);
                var requireNewDb = !dbLoaded;

                while (!dbLoaded)
                {
                    // download new database from network
                    if (!await DownloadLatestDatabase().ConfigureAwait(false))
                    {
                        await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        dbLoaded = await LoadLatestDatabase(false).ConfigureAwait(false);
                    }
                }

                var initialTimeout = requireNewDb ? TimeSpan.FromDays(24) : TimeSpan.Zero;
                _fetchTimer = new Timer(_ => PerformUpdate(), null, initialTimeout, TimeSpan.FromDays(1));
            }
        }

        async Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            await _fetchTimer.DisposeAsync();
        }

        public void Dispose()
        {
            _database?.Dispose();
            _initialLock?.Dispose();
        }
    }
}
