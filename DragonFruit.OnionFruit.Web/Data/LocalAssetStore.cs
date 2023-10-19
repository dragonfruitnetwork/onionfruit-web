// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace DragonFruit.OnionFruit.Web.Data;

internal class LocalAssetStore
{
    private readonly string _assetRoot;
    private readonly IDictionary<string, string> _assetLocations;

    public LocalAssetStore(IConfiguration configuration)
    {
        _assetRoot = configuration["Server:AssetRoot"] ?? Path.Combine(Path.GetTempPath(), "onionfruit-web-assets");
        _assetLocations = GenerateAssetTable();
    }

    /// <summary>
    /// Resolves a <see cref="fileName"/> to the currently served version
    /// </summary>
    /// <remarks>
    /// Paths returned by this accessor will use the system's path separator and will need to be checked for URL compatibility.
    /// </remarks>
    public string this[string fileName] => _assetLocations.TryGetValue(fileName, out var filePath) ? filePath : null;

    /// <summary>
    /// Creates a new asset store version, returning a container that can be used to submit files.
    /// </summary>
    public LocalAssetStoreRevision CreateNewAssetStoreRevision()
    {
        // use current timestamp for versioning
        var folderPath = Path.Combine(_assetRoot, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture));

        Directory.CreateDirectory(folderPath);
        return new LocalAssetStoreRevision(folderPath, s => _assetLocations[s.Replace('\\', '/')] = Path.Combine(folderPath, s));
    }

    /// <summary>
    /// Generates a table of files -> relative filesystem paths
    /// </summary>
    /// <returns>
    /// A <see cref="IDictionary{TKey,TValue}"/> containing a mapping of file names to relative paths
    /// </returns>
    private IDictionary<string, string> GenerateAssetTable()
    {
        var table = new ConcurrentDictionary<string, string>();

        // ensure directory exists
        Directory.CreateDirectory(_assetRoot);

        // get all folders and order by value
        foreach (var file in Directory.GetDirectories(_assetRoot).Order().SelectMany(x => Directory.GetFiles(x, "*", SearchOption.AllDirectories)))
        {
            var relPath = Path.GetRelativePath(_assetRoot, file);

            // If the path was `12345/testing/target.txt`, the key would be testing/target.txt so the versioning directory part needs extracting.
            // The GetRootFolder function will get the first path but the slash needs trimming as well, so 1 is added to the length.
            var versionPathLength = GetRootFolder(relPath).Length + 1;
            table[relPath.Substring(versionPathLength).Replace('\\', '/')] = relPath;
        }

        return table;
    }

    // https://stackoverflow.com/a/7911591
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
}

internal class LocalAssetStoreRevision
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
        if (fileName.Contains(".."))
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