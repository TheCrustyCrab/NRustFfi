using System.Runtime.InteropServices;

namespace NRustFfi.Futures;

/// <summary>
/// An async_ffi::FfiContext.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct Context
{
    internal nint WakerPtr;
}