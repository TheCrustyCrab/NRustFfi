using System.Runtime.InteropServices;
using String = NRustFfi.NativeTypes.String;

namespace NRustFfi.Futures.NativeTypes;

[StructLayout(LayoutKind.Sequential)]
public struct FutureString : IFuture<PollString, String>
{
    internal nint Fut;
    internal PollCallback Poll;
    internal DropFuture Drop;

    PollString IFuture<PollString, String>.Poll(ref Context context) => Poll(Fut, ref context);

    void IFuture<PollString, String>.Drop() => Drop(Fut);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate PollString PollCallback(nint fut, ref Context context);
}

[StructLayout(LayoutKind.Sequential)]
internal struct PollString : IPoll<String>
{
    internal PollDiscriminant State;
    internal String Value;

    readonly PollDiscriminant IPoll<String>.State => State;

    readonly String? IPoll<String>.Value => Value;
}
