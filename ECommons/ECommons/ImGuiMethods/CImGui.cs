using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace ECommons.ImGuiMethods;

public partial class CImGui
{
    [LibraryImport("cimgui")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    public static partial void igBringWindowToDisplayFront(nint ptr);

    [LibraryImport("cimgui")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    public static partial void igBringWindowToDisplayBack(nint ptr);

    [LibraryImport("cimgui")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    public static partial nint igGetCurrentWindow();

    [LibraryImport("cimgui")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    public static partial int igFindWindowDisplayIndex(nint ptr);
}
