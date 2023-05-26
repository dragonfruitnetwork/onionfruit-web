// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Runtime.CompilerServices;

namespace DragonFruit.OnionFruit.Services.LocationDb
{
    internal static class AddressUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetAddressBit(Span<byte> addressBytes, int depth)
        {
            return ((addressBytes[depth / 8] >> (7 - (depth % 8))) & 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetAddressBit(Span<byte> addressBytes, int depth, int value)
        {
            addressBytes[depth / 8] ^= (byte)((-value ^ addressBytes[depth / 8]) & (1 << (7 - (depth % 8))));
        }

        public static bool IsIPv4(ReadOnlySpan<byte> addressBytes)
        {
            for (int i = 0; i < 10; i++)
            {
                if (addressBytes[i] > 0)
                {
                    return false;
                }
            }

            for (int i = 10; i < 12; i++)
            {
                if (addressBytes[i] != 0xff)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
