using System.Runtime.InteropServices;
using NRustFfi.NativeTypes;

namespace NRustFfi.Futures.NativeTypes;

[StructLayout(LayoutKind.Sequential)]
public struct FutureUsize : IFuture<PollUsize, nuint>
{
    internal nint Fut;
    internal PollCallback Poll;
    internal DropFuture Drop;

    PollUsize IFuture<PollUsize, nuint>.Poll(ref Context context) => Poll(Fut, ref context);

    void IFuture<PollUsize, nuint>.Drop()
    {
        Drop(Fut);
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate PollUsize PollCallback(nint fut, ref Context context);
}

[StructLayout(LayoutKind.Sequential)]
internal struct PollUsize : IPoll<nuint>
{
    internal PollDiscriminant State;
    internal nuint Value;

    readonly PollDiscriminant IPoll<nuint>.State => State;

    readonly nuint? IPoll<nuint>.Value => Value;
}

[StructLayout(LayoutKind.Sequential)]
internal struct OptionUsize
{
    internal OptionDiscriminant Discriminant;
    internal nuint Value;
}


[StructLayout(LayoutKind.Sequential)]
public struct FutureOptionUsize : IFuture<PollOptionUsize, nuint>
{
    internal nint Fut;
    internal PollCallback Poll;
    internal DropFuture Drop;

    PollOptionUsize IFuture<PollOptionUsize, nuint>.Poll(ref Context context) => Poll(Fut, ref context);

    void IFuture<PollOptionUsize, nuint>.Drop() => Drop(Fut);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate PollOptionUsize PollCallback(nint fut, ref Context context);
}

[StructLayout(LayoutKind.Sequential)]
internal struct PollOptionUsize : IPoll<nuint>
{
    internal PollDiscriminant State;
    internal OptionUsize OptionalValue;

    readonly PollDiscriminant IPoll<nuint>.State => State;

    readonly nuint? IPoll<nuint>.Value => OptionalValue.Discriminant == OptionDiscriminant.Some ? OptionalValue.Value : null;
}

[StructLayout(LayoutKind.Explicit)]
internal struct ResultUsizeStaticStrSliceUnion
{
    [FieldOffset(0)] internal nuint Value;
    [FieldOffset(0)] internal StrSlice Error;
}

[StructLayout(LayoutKind.Sequential)]
internal struct ResultUsizeStaticStrSlice
{
    internal ResultDiscriminant Discriminant;
    internal ResultUsizeStaticStrSliceUnion Union;
}

[StructLayout(LayoutKind.Sequential)]
public struct FutureResultUsizeStaticStrSlice : IFuture<PollResultUsizeStaticStrSlice, ResultUsizeStaticStrSlice>
{
    internal nint Fut;
    internal PollCallback Poll;
    internal DropFuture Drop;

    PollResultUsizeStaticStrSlice IFuture<PollResultUsizeStaticStrSlice, ResultUsizeStaticStrSlice>.Poll(ref Context context) => Poll(Fut, ref context);

    void IFuture<PollResultUsizeStaticStrSlice, ResultUsizeStaticStrSlice>.Drop() => Drop(Fut);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate PollResultUsizeStaticStrSlice PollCallback(nint fut, ref Context context);
}

[StructLayout(LayoutKind.Sequential)]
internal struct PollResultUsizeStaticStrSlice : IPoll<ResultUsizeStaticStrSlice>
{
    internal PollDiscriminant State;
    internal ResultUsizeStaticStrSlice ResultValue;

    readonly PollDiscriminant IPoll<ResultUsizeStaticStrSlice>.State => State;

    readonly ResultUsizeStaticStrSlice? IPoll<ResultUsizeStaticStrSlice>.Value => ResultValue;
}