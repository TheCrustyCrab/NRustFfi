using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NRustFfi.Futures;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate nint WakerClone(nint waker);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void WakerWake(nint waker);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void WakerWakeByRef(nint waker);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void WakerDrop(nint waker);

/// <summary>
/// An async_ffi::FfiWakerVTable.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct WakerVTable
{
    internal WakerClone Clone;
    internal WakerWake Wake;
    internal WakerWakeByRef WakeByRef;
    internal WakerDrop Drop;
}

/// <summary>
/// An async_ffi::FfiWaker.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct Waker
{
    // This must be the first field in the struct to match the FfiWaker layout.
    internal nint VtablePtr;

    internal nint SelfPtr;
    internal int ReferenceCount;
    internal GCHandle GcHandleOnWake;

    internal static nint Create(Action onWake)
    {
        var gcHandleOnWake = GCHandle.Alloc(onWake);
        var vtable = new WakerVTable
        {
            Clone = CloneCallback,
            Drop = DropCallback,
            Wake = WakeCallback,
            WakeByRef = WakeByRefCallback
        };
        var vtablePtr = Marshal.AllocHGlobal(Marshal.SizeOf<WakerVTable>());
        Marshal.StructureToPtr(vtable, vtablePtr, false);

        var wakerPtr = Marshal.AllocHGlobal(Marshal.SizeOf<Waker>());
        var waker = new Waker
        {
            VtablePtr = vtablePtr,
            SelfPtr = wakerPtr,
            GcHandleOnWake = gcHandleOnWake,
            ReferenceCount = 1
        };
        Marshal.StructureToPtr(waker, wakerPtr, false);

        return wakerPtr;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ref Waker FromRawPointer(nint ptr)
    {
        unsafe
        {
            return ref Unsafe.AsRef<Waker>((void*)ptr);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Drop()
    {
        Interlocked.Decrement(ref ReferenceCount);
        if (ReferenceCount == 0)
        {
            Marshal.FreeHGlobal(VtablePtr);
            Marshal.FreeHGlobal(SelfPtr);
            GcHandleOnWake.Free();
        }
    }

    private static nint CloneCallback(nint ptr)
    {
        ref var waker = ref FromRawPointer(ptr);
        Interlocked.Increment(ref waker.ReferenceCount);
        return ptr;
    }

    private static void DropCallback(nint ptr)
    {
        ref var waker = ref FromRawPointer(ptr);
        waker.Drop();
    }

    private static void WakeCallback(nint ptr)
    {
        WakeByRefCallback(ptr);
        DropCallback(ptr);
    }

    private static void WakeByRefCallback(nint ptr)
    {
        ref var waker = ref FromRawPointer(ptr);
        var onWake = (Action)waker.GcHandleOnWake.Target!;
        onWake();
    }
}