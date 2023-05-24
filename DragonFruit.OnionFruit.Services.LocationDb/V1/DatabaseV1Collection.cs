// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using DragonFruit.OnionFruit.Services.LocationDb.Abstractions;

namespace DragonFruit.OnionFruit.Services.LocationDb.V1
{
    internal abstract class DatabaseV1Collection<TSource, TOut> : IBinarySearchable<TOut>, IEnumerable<TSource>, IDisposable
        where TOut : ISearchableItem
        where TSource : struct
    {
        protected readonly MemoryMappedViewAccessor View;
        protected readonly IStringPool Pool;

        private readonly int _entitySize;

        protected DatabaseV1Collection(MemoryMappedViewAccessor view, IStringPool pool)
        {
            _entitySize = Unsafe.SizeOf<TSource>();

            View = view;
            Pool = pool;
            Count = (int)View.Capacity / _entitySize;
        }

        protected abstract TOut FromNative(TSource source);

        public int Count { get; }

        public unsafe TOut this[int index]
        {
            get
            {
                View.Read(index * _entitySize, out TSource data);
                return FromNative(data);
            }
        }

        IEnumerator<TSource> IEnumerable<TSource>.GetEnumerator()
        {
            return new ViewEnumerator<TSource>(View);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<TSource>)this).GetEnumerator();
        }

        public virtual void Dispose()
        {
            View.Dispose();
        }
    }
}
