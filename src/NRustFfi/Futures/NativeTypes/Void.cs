using System.Runtime.InteropServices;

namespace NRustFfi.Futures.NativeTypes;

[StructLayout(LayoutKind.Sequential)]
public struct FutureVoid : IFuture<PollVoid, ValueTuple>
{
    internal nint Fut;
    internal PollCallback Poll;
    internal DropFuture Drop;

    PollVoid IFuture<PollVoid, ValueTuple>.Poll(ref Context context) => Poll(Fut, ref context);

    void IFuture<PollVoid, ValueTuple>.Drop() => Drop(Fut);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate PollVoid PollCallback(nint fut, ref Context context);
}

[StructLayout(LayoutKind.Sequential)]
internal struct PollVoid : IPoll<ValueTuple>
{
    internal PollDiscriminant State;

    readonly PollDiscriminant IPoll<ValueTuple>.State => State;

    readonly ValueTuple? IPoll<ValueTuple>.Value => default;
}