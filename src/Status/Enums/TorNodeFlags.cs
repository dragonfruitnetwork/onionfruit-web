﻿using System;

namespace DragonFruit.OnionFruit.Api.Status.Enums
{
    [Flags]
    public enum TorNodeFlags
    {
        None = 1 << 0,
        Exit = 1 << 1,
        Fast = 1 << 2,
        Guard = 1 << 3,
        Named = 1 << 4,
        Stable = 1 << 5,
        Running = 1 << 6,
        Valid = 1 << 7,
        V2Dir = 1 << 8,
        HsDir = 1 << 9
    }
}