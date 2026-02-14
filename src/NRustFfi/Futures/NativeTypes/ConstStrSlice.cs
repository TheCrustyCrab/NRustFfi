using System.Runtime.InteropServices;
using NRustFfi.NativeTypes;

namespace NRustFfi.Futures.NativeTypes;

[StructLayout(LayoutKind.Sequential)]
public struct FutureConstStrSlice : IFuture<PollConstStrSlice, StrSlice>
{
    internal nint Fut;
    internal PollCallback Poll;
    internal DropFuture Drop;

    PollConstStrSlice IFuture<PollConstStrSlice, StrSlice>.Poll(ref Context context) => Poll(Fut, ref context);

    void IFuture<PollConstStrSlice, StrSlice>.Drop() => Drop(Fut);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate PollConstStrSlice PollCallback(nint fut, ref Context context);
}

[StructLayout(LayoutKind.Sequential)]
internal struct PollConstStrSlice : IPoll<StrSlice>
{
    internal PollDiscriminant State;
    internal StrSlice Value;

    readonly PollDiscriminant IPoll<StrSlice>.State => State;

    readonly StrSlice? IPoll<StrSlice>.Value => Value;
}