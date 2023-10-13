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
public struct NetworkEntry
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] network;
    public byte cidr;
        
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public byte[] country_code;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct NetworkSortResult
{
    public IntPtr v4Entries;
    public nint v4Count;

    public IntPtr v6Entries;
    public nint v6Count;
}