using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NRustFfi.NativeTypes;

/// <summary>
/// A uuid::Uuid.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct Uuid
{
    public byte b1;
    public byte b2;
    public byte b3;
    public byte b4;
    public byte b5;
    public byte b6;
    public byte b7;
    public byte b8;
    public byte b9;
    public byte b10;
    public byte b11;
    public byte b12;
    public byte b13;
    public byte b14;
    public byte b15;
    public byte b16;

    /// <summary>
    /// Creates a <see cref="Uuid"/> from a <see cref="Guid"/>.
    /// </summary>
    /// <param name="guid">A Guid</param>
    /// <returns>A Uuid</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Uuid FromGuid(Guid guid)
    {
        Span<byte> bytes = stackalloc byte[16];
        guid.TryWriteBytes(bytes);
        if (BitConverter.IsLittleEndian)
        {
            var b1 = bytes[3];
            var b2 = bytes[2];
            var b3 = bytes[1];
            var b4 = bytes[0];
            var b5 = bytes[5];
            var b6 = bytes[4];
            var b7 = bytes[7];
            var b8 = bytes[6];
            bytes[0] = b1;
            bytes[1] = b2;
            bytes[2] = b3;
            bytes[3] = b4;
            bytes[4] = b5;
            bytes[5] = b6;
            bytes[6] = b7;
            bytes[7] = b8;
        }
        return MemoryMarshal.Read<Uuid>(bytes);
    }

    /// <summary>
    /// Creates a <see cref="Uuid"/> from a <see cref="string"/>.
    /// </summary>
    /// <param name="guid">A Guid as string</param>
    /// <returns>A Uuid</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Uuid FromGuid(string guid) => FromGuid(Guid.Parse(guid));
}