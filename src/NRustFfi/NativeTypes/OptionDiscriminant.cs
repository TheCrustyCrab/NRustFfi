namespace NRustFfi.NativeTypes
{
    /// <summary>
    /// The discriminant of an FFI-safe Option.
    /// </summary>
    internal enum OptionDiscriminant : byte
    {
        None,
        Some
    }
}
