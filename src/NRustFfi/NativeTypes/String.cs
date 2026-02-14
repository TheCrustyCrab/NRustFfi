using System.Runtime.InteropServices;

namespace NRustFfi.NativeTypes;

public delegate void DropString(String s);

/// <summary>
/// An owned String from Rust.
/// Must be dropped after usage.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct String
{
    internal nint Ptr;
    internal nint Len;
    internal nint Capacity;

    /// <summary>
    /// Interprets the string slice as a read-only span to UTF-8 text, which can be reliably read as long as the unmanaged string is not freed.
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
    /// Frees the unmanaged string and returns a managed string instead. <br/>
    /// The <see cref="DropRustString"/> argument should be the unmanaged function responsible for dropping the Rust String, <br/>
    /// for example by calling <see href="https://doc.rust-lang.org/std/string/struct.String.html#method.from_raw_parts">String::from_raw_parts</see>.
    /// </summary>
    /// <param name="drop"></param>
    /// <returns>A managed string</returns>
    public string IntoString(DropString drop)
    {
        var result = Marshal.PtrToStringUTF8(Ptr, (int)Len);
        drop(this);
        return result;
    }
}
