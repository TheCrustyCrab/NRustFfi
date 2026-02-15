using System.Runtime.InteropServices;
using NRustFfi.NativeTypes;

namespace NRustFfi.Futures.NativeTypes;

[StructLayout(LayoutKind.Sequential)]
public struct FutureStaticStrSlice : IFuture<PollStaticStrSlice, StrSlice>
{
    internal nint Fut;
    internal PollCallback Poll;
    internal DropFuture Drop;

    PollStaticStrSlice IFuture<PollStaticStrSlice, StrSlice>.Poll(ref Context context) => Poll(Fut, ref context);

    void IFuture<PollStaticStrSlice, StrSlice>.Drop() => Drop(Fut);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate PollStaticStrSlice PollCallback(nint fut, ref Context context);
}

[StructLayout(LayoutKind.Sequential)]
internal struct PollStaticStrSlice : IPoll<StrSlice>
{
    internal PollDiscriminant State;
    internal StrSlice Value;

    readonly PollDiscriminant IPoll<StrSlice>.State => State;

    readonly StrSlice? IPoll<StrSlice>.Value => Value;
}