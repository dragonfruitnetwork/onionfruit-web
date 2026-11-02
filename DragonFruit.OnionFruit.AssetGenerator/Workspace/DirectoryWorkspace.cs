// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using DragonFruit.OnionFruit.AssetGenerator.Generators.Core.Abstractions;

namespace DragonFruit.OnionFruit.AssetGenerator.Workspace;

public sealed class DirectoryWorkspace : IWorkspace, IDisposable
{
    private readonly bool _deleteOnDispose;

    private DirectoryWorkspace(string root, bool deleteOnDispose)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);

        RootPath = Path.GetFullPath(root);
        _deleteOnDispose = deleteOnDispose;

        Directory.CreateDirectory(RootPath);
    }

    public string RootPath { get; }

    /// <summary>
    /// Creates a workspace in a uniquely-named directory, optionally beneath <paramref name="parentDirectory"/>.
    /// </summary>
    public static DirectoryWorkspace CreateTemporary(string? parentDirectory = null, string? name = null, bool deleteOnDispose = true)
    {
        var parent = string.IsNullOrWhiteSpace(parentDirectory)
            ? Path.Combine(Path.GetTempPath(), "onionfruit-asset-generator")
            : Path.GetFullPath(parentDirectory);

        var directoryName = string.IsNullOrWhiteSpace(name)
            ? Guid.NewGuid().ToString("N")
            : $"{name}-{Guid.NewGuid():N}";

        return new DirectoryWorkspace(Path.Combine(parent, directoryName), deleteOnDispose);
    }

    public void Dispose()
    {
        if (!_deleteOnDispose || !Directory.Exists(RootPath))
        {
            return;
        }

        try
        {
            Directory.Delete(RootPath, recursive: true);
        }
        catch
        {
            // best-effort cleanup, matching the non-throwing semantics of IDisposable teardown
        }
    }

    public async Task WriteAsync(string key, byte[] contents, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(contents);

        await using var file = await CreateFileAsync(key, ct);
        await file.WriteAsync(contents, ct);
        await file.FlushAsync(ct);
    }

    public Task<FileStream> CreateFileAsync(string key, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var path = ResolvePath(key);
        var directory = Path.GetDirectoryName(path)!;
        var temporaryPath = Path.Combine(directory, $".{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");

        Directory.CreateDirectory(directory);

        return Task.FromResult<FileStream>(new StagedFileStream(temporaryPath, path));
    }

    private string ResolvePath(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var relativePath = key
            .Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar);

        if (Path.IsPathRooted(relativePath) || relativePath.EndsWith(Path.DirectorySeparatorChar))
        {
            throw new ArgumentException($"Workspace keys must be relative paths to a file (got '{key}').", nameof(key));
        }

        var path = Path.GetFullPath(Path.Combine(RootPath, relativePath));
        var rootPrefix = RootPath.EndsWith(Path.DirectorySeparatorChar) ? RootPath : RootPath + Path.DirectorySeparatorChar;
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        if (!path.StartsWith(rootPrefix, comparison))
        {
            throw new ArgumentException($"Workspace key '{key}' resolves outside the workspace root.", nameof(key));
        }

        return path;
    }
}