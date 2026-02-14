using System.Runtime.InteropServices;

namespace NRustFfi.Futures.NativeTypes;

[StructLayout(LayoutKind.Sequential)]
public struct FutureU32 : IFuture<PollU32, uint>
{
    internal nint Fut;
    internal PollCallback Poll;
    internal DropFuture Drop;

    PollU32 IFuture<PollU32, uint>.Poll(ref Context context) => Poll(Fut, ref context);

    void IFuture<PollU32, uint>.Drop() => Drop(Fut);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate PollU32 PollCallback(nint fut, ref Context context);
}

[StructLayout(LayoutKind.Sequential)]
internal struct PollU32 : IPoll<uint>
{
    internal PollDiscriminant State;
    internal uint Value;

    readonly PollDiscriminant IPoll<uint>.State => State;

    readonly uint? IPoll<uint>.Value => Value;
}