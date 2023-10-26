using System;

namespace ECommons;

public static unsafe class PointerHelpers
{
    public static T* As<T>(this IntPtr ptr) where T:unmanaged
    {
        return (T*)ptr;
    }
}
