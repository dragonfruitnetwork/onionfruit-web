// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

namespace DragonFruit.OnionFruit.AssetGenerator.Workspace;

/// <summary>
/// A <see cref="FileStream"/> that writes to a temporary path and atomically promotes it to
/// the final path when the stream is disposed successfully. If any write/flush fails, or the
/// stream is finalized without being disposed, the temporary file is removed instead.
/// </summary>
internal sealed class StagedFileStream : FileStream
{
    private readonly string _temporaryPath;
    private readonly string _finalPath;

    private bool _failed;
    private bool _completed;

    internal StagedFileStream(string temporaryPath, string finalPath)
        : base(temporaryPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.Asynchronous)
    {
        _temporaryPath = temporaryPath;
        _finalPath = finalPath;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        try
        {
            base.Write(buffer, offset, count);
        }
        catch
        {
            _failed = true;
            throw;
        }
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        try
        {
            base.Write(buffer);
        }
        catch
        {
            _failed = true;
            throw;
        }
    }

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        try
        {
            await base.WriteAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            _failed = true;
            throw;
        }
    }

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        try
        {
            await base.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            _failed = true;
            throw;
        }
    }

    public override void WriteByte(byte value)
    {
        try
        {
            base.WriteByte(value);
        }
        catch
        {
            _failed = true;
            throw;
        }
    }

    public override void SetLength(long value)
    {
        try
        {
            base.SetLength(value);
        }
        catch
        {
            _failed = true;
            throw;
        }
    }

    public override void Flush()
    {
        try
        {
            base.Flush();
        }
        catch
        {
            _failed = true;
            throw;
        }
    }

    public override async Task FlushAsync(CancellationToken cancellationToken)
    {
        try
        {
            await base.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            _failed = true;
            throw;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (!disposing)
        {
            base.Dispose(false);
            DeleteTemporaryFile();
            return;
        }

        try
        {
            base.Dispose(true);
        }
        catch
        {
            _failed = true;
            throw;
        }
        finally
        {
            Complete();
        }
    }

    public override async ValueTask DisposeAsync()
    {
        try
        {
            await base.DisposeAsync().ConfigureAwait(false);
        }
        catch
        {
            _failed = true;
            throw;
        }
        finally
        {
            Complete();
            GC.SuppressFinalize(this);
        }
    }

    private void Complete()
    {
        if (_completed)
        {
            return;
        }

        _completed = true;

        if (_failed)
        {
            DeleteTemporaryFile();
            return;
        }

        try
        {
            // Both paths live in the same directory, so this is a same-volume
            // rename/move — the final key is either the old asset or the new one.
            File.Move(_temporaryPath, _finalPath, overwrite: true);
        }
        catch
        {
            DeleteTemporaryFile();
            throw;
        }
    }

    private void DeleteTemporaryFile()
    {
        try
        {
            File.Delete(_temporaryPath);
        }
        catch
        {
            // Best-effort cleanup — the final path was never written to.
        }
    }
}
