// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DragonFruit.OnionFruit.Web.Worker.Native;

public static partial class NativeMethods
{
    private const string LibraryName = "onionfruit_worker_native";

    [LibraryImport(LibraryName, EntryPoint = "sort_network_entries")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void PerformNetworkSort(in IntPtr entries, nint length, out NetworkSortResult result);

    [LibraryImport(LibraryName, EntryPoint = "free_sort_result")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void ClearSortResult(ref NetworkSortResult ptr);
}