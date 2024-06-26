﻿// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.Runtime.InteropServices;

namespace DragonFruit.OnionFruit.Web.Worker.Native;

public static class NativeMethods
{
    private const string LibraryName = "onionfruit_worker_native";

    [DllImport(LibraryName, EntryPoint = "sort_network_entries", CallingConvention = CallingConvention.Cdecl)]
    public static extern void PerformNetworkSort(IntPtr entries, nint length, out NetworkSortResult result);

    [DllImport(LibraryName, EntryPoint = "free_sort_result", CallingConvention = CallingConvention.Cdecl)]
    public static extern void ClearSortResult(ref NetworkSortResult ptr);
}