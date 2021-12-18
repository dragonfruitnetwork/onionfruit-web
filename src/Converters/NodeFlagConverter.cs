﻿// OnionFruit API/Tooling Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Collections.Generic;
using System.Linq;
using DragonFruit.OnionFruit.Api.Enums;

namespace DragonFruit.OnionFruit.Api.Converters
{
    internal static class NodeFlagConverter
    {
        public static TorNodeFlags Parse(IEnumerable<string> flags)
        {
            var data = flags.Aggregate(TorNodeFlags.None, (a, b) =>
            {
                if (Enum.TryParse(b, out TorNodeFlags flag))
                {
                    return a | flag;
                }

                return flag;
            });

            // if there are flags, untoggle the none flag
            return data > TorNodeFlags.None ? data & ~TorNodeFlags.None : data;
        }

        public static string[] ToString(TorNodeFlags flags)
        {
            return Enum.GetValues(typeof(TorNodeFlags)).Cast<TorNodeFlags>()
                       .Where(x => (flags & x) == x)
                       .Select(x => x.ToString())
                       .ToArray();
        }
    }
}
