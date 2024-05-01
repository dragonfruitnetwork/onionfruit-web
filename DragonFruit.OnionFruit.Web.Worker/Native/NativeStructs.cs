// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
namespace DragonFruit.OnionFruit.Web.Worker.Native;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct IPv4NetworkRange
{
    public fixed byte start_address[4];
    public fixed byte end_address[4];

    public fixed byte country_code[2];
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct IPv6NetworkRange
{
    public fixed byte start_address[16];
    public fixed byte end_address[16];

    public fixed byte country_code[2];
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct NetworkEntry
{
    public fixed byte network[16];
    public byte cidr;
    public fixed byte country_code[2];
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct NetworkSortResult
{
    public IntPtr v4Entries;
    public nint v4Capacity;
    public nint v4Count;

    public IntPtr v6Entries;
    public nint v6Capacity;
    public nint v6Count;
}