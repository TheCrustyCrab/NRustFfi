using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NRustFfi.Futures.NativeTypes;
using Xunit;

namespace NRustFfi.Tests;

internal static class NativeMethods
{
    [DllImport("nrustffi_test_lib", EntryPoint = "sleep", CallingConvention = CallingConvention.Cdecl)]
    internal static extern FutureVoid Sleep(long millis);

    [DllImport("nrustffi_test_lib", EntryPoint = "sleep_and_get_usize", CallingConvention = CallingConvention.Cdecl)]
    internal static extern FutureUsize SleepAndGetUsize(long millis);

    [DllImport("nrustffi_test_lib", EntryPoint = "sleep_and_get_u32", CallingConvention = CallingConvention.Cdecl)]
    internal static extern FutureU32 SleepAndGetU32(long millis);

    [DllImport("nrustffi_test_lib", EntryPoint = "sleep_and_get_const_str", CallingConvention = CallingConvention.Cdecl)]
    internal static extern FutureConstStrSlice SleepAndGetConstStr(long millis);

    [DllImport("nrustffi_test_lib", EntryPoint = "sleep_and_get_string", CallingConvention = CallingConvention.Cdecl)]
    internal static extern FutureString SleepAndGetString(long millis);

    [DllImport("nrustffi_test_lib", EntryPoint = "drop_string", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void DropString(NativeTypes.String s);

    [DllImport("nrustffi_test_lib", EntryPoint = "sleep_and_panic", CallingConvention = CallingConvention.Cdecl)]
    internal static extern FutureVoid SleepAndPanic(long millis);

    [DllImport("nrustffi_test_lib", EntryPoint = "instant_panic", CallingConvention = CallingConvention.Cdecl)]
    internal static extern FutureVoid InstantPanic();

    [DllImport("nrustffi_test_lib", EntryPoint = "sleep_and_get_option_usize", CallingConvention = CallingConvention.Cdecl)]
    internal static extern FutureOptionUsize SleepAndGetOptionUsize(long millis, bool withSome);

    [DllImport("nrustffi_test_lib", EntryPoint = "sleep_and_get_result_usize", CallingConvention = CallingConvention.Cdecl)]
    internal static extern FutureResultUsizeConstStrSlice SleepAndGetResultUsize(long millis, bool withOk);
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
    public async Task TestFutureConstStrSlice()
    {
        var s = Unsafe.SizeOf<FutureConstStrSlice>();
        Assert.Equal("A const str from Rust!", await RustFfi.RunAsync(NativeMethods.SleepAndGetConstStr(1000), TestContext.Current.CancellationToken));
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
}