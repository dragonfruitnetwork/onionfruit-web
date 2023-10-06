using System.Runtime.InteropServices;

namespace DragonFruit.OnionFruit.Web.Worker.Native;

public static class NativeMethods
{
    private const string LibraryName = "onionfruit_worker_native";

    [DllImport(LibraryName, EntryPoint = "sort_network_entries", CallingConvention = CallingConvention.Cdecl)]
    public static extern void PerformNetworkSort([In, MarshalAs(UnmanagedType.LPArray)] NetworkEntry[] entries, nint length, out NetworkSortResult result);

    [DllImport(LibraryName, EntryPoint = "free_sort_result", CallingConvention = CallingConvention.Cdecl)]
    public static extern void ClearSortResult([In] ref NetworkSortResult ptr);
}