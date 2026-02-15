using System.Runtime.InteropServices;
using System.Text.Json;
using NRustFfi.Futures;
using NRustFfi.Futures.NativeTypes;
using NRustFfi.NativeTypes;
using String = NRustFfi.NativeTypes.String;

namespace NRustFfi;

public static class RustFfi
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public static async ValueTask RunAsync(FutureVoid future, CancellationToken token = default)
    {
        if (FutureAwaiter<FutureVoid, PollVoid, ValueTuple>.TryCompleteSynchronously(future, token, out var syncResult, out var asyncAwaiter))
            return;

        using (asyncAwaiter)
        {
            await asyncAwaiter!;
        }
    }

    public static async ValueTask<nuint> RunAsync(FutureUsize future, CancellationToken token = default)
    {
        if (FutureAwaiter<FutureUsize, PollUsize, nuint>.TryCompleteSynchronously(future, token, out var syncResult, out var asyncAwaiter))
            return syncResult!.Value;

        using (asyncAwaiter)
        {
            return (await asyncAwaiter!).Value;
        }
    }

    public static async ValueTask<nuint?> RunAsync(FutureOptionUsize future, CancellationToken token = default)
    {
        if (FutureAwaiter<FutureOptionUsize, PollOptionUsize, nuint>.TryCompleteSynchronously(future, token, out var syncResult, out var asyncAwaiter))
            return syncResult;

        using (asyncAwaiter)
        {
            return await asyncAwaiter!;
        }
    }

    public static async ValueTask<(nuint? Value, string? Error)> RunAsync(FutureResultUsizeStaticStrSlice future, CancellationToken token = default)
    {
        var s = Marshal.SizeOf<ResultUsizeStaticStrSlice>();
        ResultUsizeStaticStrSlice ? result = null;
        if (FutureAwaiter<FutureResultUsizeStaticStrSlice, PollResultUsizeStaticStrSlice, ResultUsizeStaticStrSlice>.TryCompleteSynchronously(future, token, out var syncResult, out var asyncAwaiter))
            result = syncResult!.Value;
        else
        {
            using (asyncAwaiter)
            {
                result = await asyncAwaiter!;
            }
        }

        return result.Value.Discriminant == ResultDiscriminant.Ok ? (result.Value.Union.Value, null) : (null, result.Value.Union.Error.AsString());
    }

    public static async ValueTask<uint> RunAsync(FutureU32 future, CancellationToken token = default)
    {
        if (FutureAwaiter<FutureU32, PollU32, uint>.TryCompleteSynchronously(future, token, out var syncResult, out var asyncAwaiter))
            return syncResult!.Value;

        using (asyncAwaiter)
        {
            return (await asyncAwaiter!).Value;
        }
    }

    public static async ValueTask<string> RunAsync(FutureStaticStrSlice future, CancellationToken token = default)
    {
        if (FutureAwaiter<FutureStaticStrSlice, PollStaticStrSlice, StrSlice>.TryCompleteSynchronously(future, token, out var syncResult, out var asyncAwaiter))
            return syncResult!.Value.AsString();

        using (asyncAwaiter)
        {
            return (await asyncAwaiter!).Value.AsString();
        }
    }

    /// <summary>
    /// Runs the future and frees the intermediate Rust String. <br/>
    /// The <see cref="DropString"/> argument should be the unmanaged function responsible for dropping the Rust String, <br/>
    /// for example by calling <see href="https://doc.rust-lang.org/std/string/struct.String.html#method.from_raw_parts">String::from_raw_parts</see>.
    /// </summary>
    /// <param name="future"></param>
    /// <param name="drop"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async ValueTask<string> RunAsync(FutureString future, DropString drop, CancellationToken token = default)
    {
        if (FutureAwaiter<FutureString, PollString, String>.TryCompleteSynchronously(future, token, out var syncResult, out var asyncAwaiter))
            return syncResult!.Value.IntoString(drop);

        using (asyncAwaiter)
        {
            return (await asyncAwaiter!).Value.IntoString(drop);
        }
    }

    /// <summary>
    /// Runs the future, deserializes the result as <typeparamref name="T"/> and frees the intermediate Rust String. <br/>
    /// The <see cref="DropString"/> argument should be the unmanaged function responsible for dropping the Rust String, <br/>
    /// for example by calling <see href="https://doc.rust-lang.org/std/string/struct.String.html#method.from_raw_parts">String::from_raw_parts</see>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="func"></param>
    /// <param name="drop"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async ValueTask<T> RunAsync<T>(FutureString future, DropString drop, CancellationToken token = default)
    {
        if (!FutureAwaiter<FutureString, PollString, String>.TryCompleteSynchronously(future, token, out var rustString, out var asyncAwaiter))
        {
            using (asyncAwaiter)
            {
                rustString = await asyncAwaiter!;
            }
        }

        var span = rustString!.Value.AsReadOnlySpan();
        var deserializedResult = JsonSerializer.Deserialize<T>(span, _jsonSerializerOptions);
        drop(rustString!.Value);
        return deserializedResult!;
    }
}
