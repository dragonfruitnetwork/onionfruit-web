// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Buffers.Binary;
using System.Collections;
using DragonFruit.OnionFruit.Services.LocationDb.Abstractions;

namespace DragonFruit.OnionFruit.Services.LocationDb
{
    internal static class BinaryUtils
    {
        public static uint EnsureEndianness(uint value) => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value;
        public static ulong EnsureEndianness(ulong value) => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value;
        public static ushort EnsureEndianness(ushort value) => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value;

        /// <summary>
        /// Performs a binary search using the provided <see cref="accessor"/>
        /// </summary>
        /// <remarks>
        /// Based on the .NET binary search function (https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/Array.cs,b92d187c91d4c9a9)
        /// </remarks>
        public static T BinarySearch<T, TKey>(int count, Func<uint, T> accessor, TKey target) where T : class, ISearchableItem<TKey>
        {
            var lo = 0;
            var hi = count - 1;
            var comparer = Comparer.DefaultInvariant;

            while (lo <= hi)
            {
                // i = median between the two numbers
                var i = lo + ((hi - lo) >> 1);
                int c;

                ISearchableItem<TKey> item;

                try
                {
                    item = accessor.Invoke((uint)i);
                    c = comparer.Compare(item.Key, target);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException($"Comparison failed: {e.Message}", e);
                }

                switch (c)
                {
                    case 0:
                        return (T)item;

                    case < 0:
                        lo = i + 1;
                        break;

                    default:
                        hi = i - 1;
                        break;
                }
            }

            return null;
        }
    }
}
