namespace NRustFfi.Futures;

/// <summary>
/// An async_ffi::FfiPoll.
/// </summary>
/// <typeparam name="TOut"></typeparam>
internal interface IPoll<TOut> where TOut : struct
{
    /// <summary>
    /// Discriminant.
    /// </summary>
    internal PollDiscriminant State { get; }

    /// <summary>
    /// Value when <see cref="State"/> is <see cref="PollDiscriminant.Ready"/>.
    /// </summary>
    internal TOut? Value { get; }
}