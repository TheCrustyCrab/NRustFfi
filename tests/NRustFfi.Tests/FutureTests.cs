using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NRustFfi.Futures.NativeTypes;
using Xunit;

namespace NRustFfi.Tests;

internal static class NativeMethods
{
#if OS_WINDOWS
    const string LibraryName = "nrustffi_test_lib";
#else
    const string LibraryName = "libnrustffi_test_lib";
#endif

    [DllImport(LibraryName, EntryPoint = "sleep", CallingConvention = CallingConvention.Cdecl)]
    internal static extern FutureVoid Sleep(long millis);

    [DllImport(LibraryName, EntryPoint = "sleep_and_get_usize", CallingConvention = CallingConvention.Cdecl)]
    internal static extern FutureUsize SleepAndGetUsize(long millis);

    [DllImport(LibraryName, EntryPoint = "sleep_and_get_u32", CallingConvention = CallingConvention.Cdecl)]
    internal static extern FutureU32 SleepAndGetU32(long millis);

    [DllImport(LibraryName, EntryPoint = "sleep_and_get_static_str", CallingConvention = CallingConvention.Cdecl)]
    internal static extern FutureStaticStrSlice SleepAndGetStaticStr(long millis);

    [DllImport(LibraryName, EntryPoint = "sleep_and_get_string", CallingConvention = CallingConvention.Cdecl)]
    internal static extern FutureString SleepAndGetString(long millis);

    [DllImport(LibraryName, EntryPoint = "drop_string", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void DropString(NativeTypes.String s);

    [DllImport(LibraryName, EntryPoint = "sleep_and_panic", CallingConvention = CallingConvention.Cdecl)]
    internal static extern FutureVoid SleepAndPanic(long millis);

    [DllImport(LibraryName, EntryPoint = "instant_panic", CallingConvention = CallingConvention.Cdecl)]
    internal static extern FutureVoid InstantPanic();

    [DllImport(LibraryName, EntryPoint = "sleep_and_get_option_usize", CallingConvention = CallingConvention.Cdecl)]
    internal static extern FutureOptionUsize SleepAndGetOptionUsize(long millis, bool withSome);

    [DllImport(LibraryName, EntryPoint = "sleep_and_get_result_usize", CallingConvention = CallingConvention.Cdecl)]
    internal static extern FutureResultUsizeStaticStrSlice SleepAndGetResultUsize(long millis, bool withOk);
}

public class FutureTests
{
    [Fact]
    public async Task TestFutureVoid()
    {
        await RustFfi.RunAsync(NativeMethods.Sleep(1000), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestFutureUsize()
    {
        Assert.Equal((nuint)123, await RustFfi.RunAsync(NativeMethods.SleepAndGetUsize(1000), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestFutureU32()
    {
        Assert.Equal((uint)123, await RustFfi.RunAsync(NativeMethods.SleepAndGetU32(1000), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestFutureStaticStrSlice()
    {
        var s = Unsafe.SizeOf<FutureStaticStrSlice>();
        Assert.Equal("A static str from Rust!", await RustFfi.RunAsync(NativeMethods.SleepAndGetStaticStr(1000), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestFutureString()
    {
        Assert.Equal("An owned String from Rust!", await RustFfi.RunAsync(NativeMethods.SleepAndGetString(1000), NativeMethods.DropString, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestFutureVoid_DelayedPanic_ThrowsException()
    {
        var exception = await Assert.ThrowsAsync<Exception>(() => RustFfi.RunAsync(NativeMethods.SleepAndPanic(1000), TestContext.Current.CancellationToken).AsTask());
        Assert.Equal("A panic occurred while polling the future.", exception.Message);
    }

    [Fact]
    public async Task TestFutureVoid_InstantPanic_ThrowsException()
    {
        var exception = await Assert.ThrowsAsync<Exception>(() => RustFfi.RunAsync(NativeMethods.InstantPanic(), TestContext.Current.CancellationToken).AsTask());
        Assert.Equal("A panic occurred while polling the future.", exception.Message);
    }

    [Fact]
    public async Task TestFutureOptionUsize()
    {
        Assert.Equal((nuint)123, await RustFfi.RunAsync(NativeMethods.SleepAndGetOptionUsize(1000, withSome: true), TestContext.Current.CancellationToken));
        Assert.Null(await RustFfi.RunAsync(NativeMethods.SleepAndGetOptionUsize(1000, withSome: false), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestFutureResultUsize()
    {
        Assert.Equal((123, null), await RustFfi.RunAsync(NativeMethods.SleepAndGetResultUsize(1000, withOk: true), TestContext.Current.CancellationToken));
        Assert.Equal((null, "An error occurred"), await RustFfi.RunAsync(NativeMethods.SleepAndGetResultUsize(1000, withOk: false), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestFutureUsize_Cancellation()
    {
        var cts = new CancellationTokenSource();
        _ = Task.Run(async () =>
        {
            await Task.Delay(500);
            cts.Cancel();
        }, TestContext.Current.CancellationToken);
        await Assert.ThrowsAsync<OperationCanceledException>(async () => await RustFfi.RunAsync(NativeMethods.SleepAndGetUsize(1000), cts.Token));
    }
}