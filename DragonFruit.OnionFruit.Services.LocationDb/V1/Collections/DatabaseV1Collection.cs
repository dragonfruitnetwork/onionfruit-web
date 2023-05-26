// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;

namespace DragonFruit.OnionFruit.Services.LocationDb.V1.Collections
{
    internal abstract class DatabaseV1Collection<T> : IEnumerable<T>, IDisposable where T : struct
    {
        protected readonly MemoryMappedViewAccessor View;

        private readonly int _entitySize;

        protected DatabaseV1Collection(MemoryMappedViewAccessor view)
        {
            _entitySize = Unsafe.SizeOf<T>();

            View = view;
            Count = (int)View.Capacity / _entitySize;
        }

        public int Count { get; }

        protected internal T ElementAt(uint index)
        {
            View.Read(index * _entitySize, out T data);
            return data;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new ViewEnumerator<T>(View);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>)this).GetEnumerator();
        }

        public virtual void Dispose()
        {
            View.Dispose();
        }
    }
}
