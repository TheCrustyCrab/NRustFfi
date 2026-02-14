namespace NRustFfi.Futures;

/// <summary>
/// An async_ffi::FfiFuture.
/// </summary>
/// <typeparam name="TPoll"></typeparam>
/// <typeparam name="TOut"></typeparam>
internal interface IFuture<TPoll, TOut>
    where TPoll : IPoll<TOut>
    where TOut : struct
{
    internal TPoll Poll(ref Context context);
    internal void Drop();
}