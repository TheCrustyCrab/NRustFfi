namespace NRustFfi.Futures;

/// <summary>
/// The discriminant of async_ffi::FfiPoll.
/// </summary>
internal enum PollDiscriminant : byte
{
    Ready,
    Pending,
    Panicked
}