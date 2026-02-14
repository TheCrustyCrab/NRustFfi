using System.Runtime.InteropServices;
using System.Text;

namespace NRustFfi.NativeTypes;

/// <summary>
/// A string slice from or to Rust.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct StrSlice : IDisposable
{
    internal readonly nint Ptr;
    internal readonly nint Len;

    private StrSlice(nint ptr, nint len)
    {
        Ptr = ptr;
        Len = len;
    }

    /// <summary>
    /// Interprets the string slice as a read-only span to UTF-8 text.
    /// </summary>
    /// <returns>A read-only span to UTF-8 text</returns>
    public readonly ReadOnlySpan<byte> AsReadOnlySpan()
    {
        unsafe
        {
            return new ReadOnlySpan<byte>((void*)Ptr, (int)Len);
        }
    }

    /// <summary>
    /// Copies the string slice in a managed string.
    /// </summary>
    /// <returns>A managed string</returns>
    public readonly string AsString() => Marshal.PtrToStringUTF8(Ptr, (int)Len);

    /// <summary>
    /// A .NET-managed string slice to Rust, which must be disposed after usage.
    /// </summary>
    /// <param name="s">A string</param>
    /// <returns>A string slice</returns>
    public static StrSlice FromString(string s)
    {
        var len = Encoding.UTF8.GetByteCount(s);
        var ptr = Marshal.AllocHGlobal(len);
        unsafe
        {
            Span<byte> bytes = new((void*)ptr, len);
            Encoding.UTF8.GetBytes(s, bytes);
            return new(ptr, len);
        }
    }

    /// <summary>
    /// Frees the unmanaged UTF-8 string.
    /// This method should only be called if the instance was created on the .NET side using <see cref="FromString(string)"./>
    /// </summary>
    public void Dispose() => Marshal.FreeHGlobal(Ptr);
}