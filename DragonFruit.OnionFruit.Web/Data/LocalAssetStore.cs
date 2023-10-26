﻿// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace DragonFruit.OnionFruit.Web.Data;

public record LocalAssetInfo(string Name, string VersionedPath, DateTimeOffset CreatedAt);

public class LocalAssetStore : IDisposable
{
    private const int ExpiryThreshold = 5;

    private readonly string _assetRoot;
    private readonly ISet<string> _accessibleFilePaths;
    private readonly FileSystemWatcher _assetFileWatcher;
    private readonly IDictionary<string, FileInfo> _activeAssetMap;

    public LocalAssetStore(IConfiguration configuration)
    {
        var root = configuration["Server:AssetRoot"];
        _assetRoot = string.IsNullOrEmpty(root) ? Path.Combine(Path.GetTempPath(), "onionfruit-web-assets") : Path.GetFullPath(root);

        // create variables
        _accessibleFilePaths = new HashSet<string>();
        _activeAssetMap = new ConcurrentDictionary<string, FileInfo>();
        _assetFileWatcher = new FileSystemWatcher
        {
            Path = _assetRoot,
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName
        };

        // directory deletions don't cause a cascade of events for each file, but as we delete individual files this isn't really an issue.
        // directories aren't stored in the set so removal is perfectly fine.
        _assetFileWatcher.Deleted += (_, e) => _accessibleFilePaths.Remove(e.FullPath);
        _assetFileWatcher.Created += (_, e) => _accessibleFilePaths.Add(e.FullPath);
        _assetFileWatcher.Renamed += (_, e) =>
        {
            _accessibleFilePaths.Remove(e.OldFullPath);
            _accessibleFilePaths.Add(e.FullPath);
        };

        PopulateAssetTables();

        // enable watcher events after populating table
        _assetFileWatcher.EnableRaisingEvents = true;
    }

    public Stream GetReadableFileStream(string relPath)
    {
        var fullPath = Path.GetFullPath(Path.Combine(_assetRoot, relPath));

        // check the full path against a list of files that can be served
        if (!_accessibleFilePaths.Contains(fullPath))
        {
            return null;
        }

        return File.Exists(fullPath) ? new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan) : null;
    }

    /// <summary>
    /// Gets the active path of the requested <see cref="fileName"/> in URL form.
    /// </summary>
    /// <param name="fileName">The name of the file to request. If there is a path separator it must be entered as a forward-slash</param>
    /// <example>
    /// Requesting "legacy/geoip" could return an asset info object with the versioned path "aa00/legacy/geoip" as that is the version of the file being currently served to users.
    /// </example>
    public LocalAssetInfo GetAssetInfo(string fileName)
    {
        if (!_activeAssetMap.TryGetValue(fileName, out var fileInfo))
        {
            return null;
        }

        return new LocalAssetInfo(fileName, Path.GetRelativePath(_assetRoot, fileInfo.FullName).Replace('\\', '/'), fileInfo.CreationTimeUtc);
    }

    /// <summary>
    /// Creates a new asset store version, returning a container that can be used to submit files.
    /// </summary>
    public LocalAssetStoreRevision CreateNewAssetStoreRevision(string revisionId)
    {
        // use current timestamp for versioning
        var folderPath = Path.Combine(_assetRoot, revisionId);

        Directory.CreateDirectory(folderPath);
        return new LocalAssetStoreRevision(folderPath, s => SetFileInfo(s, Path.Combine(folderPath, s)));
    }

    /// <summary>
    /// Removes assets that are no longer accessible by a user
    /// </summary>
    public int DeleteOrphanedAssets()
    {
        var filesDeleted = 0;
        var activeFiles = _activeAssetMap.Values.Select(x => x.FullName);

        // delete all files that are no longer used and have been preserved for the minimum expiry timespan
        foreach (var oldFile in Directory.GetFiles(_assetRoot, "*", SearchOption.AllDirectories).Except(activeFiles))
        {
            if (File.GetCreationTime(oldFile).AddDays(ExpiryThreshold) > DateTime.Now)
            {
                continue;
            }

            File.Delete(oldFile);
            filesDeleted++;
        }

        if (filesDeleted > 0)
        {
            DeleteEmptyDirectories(_assetRoot);
        }

        return filesDeleted;
    }

    /// <summary>
    /// Populates a table of request paths to relative paths (on filesystem against the provided asset root), along with a set of all files that can be downloaded by a client.
    /// </summary>
    private void PopulateAssetTables()
    {
        // ensure directory exists
        Directory.CreateDirectory(_assetRoot);

        // get all folders and order by value
        foreach (var file in Directory.GetDirectories(_assetRoot).Order().SelectMany(x => Directory.GetFiles(x, "*", SearchOption.AllDirectories)))
        {
            _accessibleFilePaths.Add(file);

            // If the path was `12345/testing/target.txt`, the key would be testing/target.txt so the versioning directory part needs extracting.
            // The GetRootFolder function will get the first path but the slash needs trimming as well, so 1 is added to the length.
            var relPath = Path.GetRelativePath(_assetRoot, file);
            var versionPathLength = GetRootFolder(relPath).Length + 1;

            SetFileInfo(relPath.Substring(versionPathLength), file);
        }
    }

    /// <summary>
    /// Recursively deletes empty directories given a starting location.
    /// </summary>
    /// <param name="startLocation">The directory to start at</param>
    /// <remarks>Adapted from https://stackoverflow.com/a/2811654 with related comments/optimisations applied</remarks>
    private static void DeleteEmptyDirectories(string startLocation)
    {
        foreach (var directory in Directory.GetDirectories(startLocation))
        {
            DeleteEmptyDirectories(directory);

            if (!Directory.EnumerateFileSystemEntries(directory).Any())
            {
                Directory.Delete(directory, false);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetFileInfo(string requestSubpath, string filePath)
    {
        // request urls use forward slashes
        _activeAssetMap[requestSubpath.Replace('\\', '/')] = new FileInfo(filePath);
    }

    /// <summary>
    /// Returns the root of a relative path
    /// </summary>
    /// <param name="path"></param>
    /// <remarks>Based on the answer https://stackoverflow.com/a/7911591</remarks>
    private static string GetRootFolder(string path)
    {
        while (true)
        {
            var temp = Path.GetDirectoryName(path);

            if (string.IsNullOrEmpty(temp))
            {
                break;
            }

            path = temp;
        }

        return path;
    }

    public void Dispose()
    {
        _assetFileWatcher?.Dispose();
    }
}

public class LocalAssetStoreRevision
{
    private readonly string _basePath;
    private readonly Action<string> _promoteItemCallback;

    public LocalAssetStoreRevision(string basePath, Action<string> promoteItemCallback)
    {
        _basePath = basePath;
        _promoteItemCallback = promoteItemCallback;
    }

    public async Task AddFile(string fileName, Stream input)
    {
        if (fileName.Contains("..", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Directory traversal not allowed");
        }

        var fullPath = Path.Combine(_basePath, fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

        using (var file = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous))
        {
            input.Seek(0, SeekOrigin.Begin);
            await input.CopyToAsync(file).ConfigureAwait(false);
        }

        _promoteItemCallback.Invoke(fileName);
    }
}