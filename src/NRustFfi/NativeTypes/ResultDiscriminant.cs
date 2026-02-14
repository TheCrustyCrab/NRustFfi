namespace NRustFfi.NativeTypes
{
    /// <summary>
    /// The discriminant of an FFI-safe Result.
    /// </summary>
    internal enum ResultDiscriminant : byte
    {
        Ok,
        Err
    }
}
