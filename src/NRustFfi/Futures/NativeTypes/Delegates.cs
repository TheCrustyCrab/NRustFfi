using System.Runtime.InteropServices;

namespace NRustFfi.Futures.NativeTypes;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void DropFuture(nint fut);