// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Collections;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;

namespace DragonFruit.OnionFruit.Services.LocationDb
{
    internal class ViewEnumerator<T> : IEnumerator<T> where T : struct
    {
        private readonly MemoryMappedViewAccessor _accessor;
        private readonly int _originalOffset;
        private readonly int _maxOffset;
        private readonly int _chunkSize;

        private int _offset;
        private T _current;

        internal ViewEnumerator(MemoryMappedViewAccessor accessor, int skip = 0, int? count = null)
        {
            _accessor = accessor;

            _chunkSize = Unsafe.SizeOf<T>();
            _originalOffset = skip * _chunkSize;
            _maxOffset = _originalOffset + count * _chunkSize ?? (int)accessor.Capacity;

            Reset();
        }

        public bool MoveNext()
        {
            if (_offset >= _maxOffset)
            {
                return false;
            }

            _accessor.Read(_offset, out _current);
            _offset += _chunkSize;

            return true;
        }

        public void Reset()
        {
            _offset = _originalOffset;
            _current = default;
        }

        public T Current => _current;

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            // disposal of view handler is managed elsewhere
        }
    }
}
