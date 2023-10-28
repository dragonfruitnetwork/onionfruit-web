// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.IO;
using System.Threading.Tasks;

namespace DragonFruit.OnionFruit.Web.Data;

public class LocalAssetStoreRevision
{
    private readonly string _basePath;
    private readonly Action<string> _promoteItemCallback;

    public LocalAssetStoreRevision(string basePath, Action<string> promoteItemCallback)
    {
        _basePath = $"{basePath.TrimEnd('/')}/";
        _promoteItemCallback = promoteItemCallback;
    }

    public async Task AddFile(string fileName, Stream input)
    {
        var fullPath = Path.Combine(_basePath, fileName);

        if (!fullPath.StartsWith(_basePath, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"{nameof(fileName)} traverses directories, which is not allowed");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

        using (var file = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous))
        {
            input.Seek(0, SeekOrigin.Begin);
            await input.CopyToAsync(file).ConfigureAwait(false);
        }

        _promoteItemCallback.Invoke(fileName);
    }
}