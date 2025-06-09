namespace Dbarone.Net.Database;

/// <summary>
/// Represents a variable-length unsigned integer using base-128 representation.
/// https://en.wikipedia.org/wiki/Variable-length_quantity
/// </summary>
public struct VarInt
{
    /// <summary>
    /// The integer value of the VarInt. 
    /// </summary>
    public ulong Value { get; set; } = 0;

    /// <summary>
    /// The byte[] array representation of the VarInt value.
    /// </summary>
    public byte[] Bytes { get; set; } = new byte[0];

    /// <summary>
    /// The length in bytes that the VarInt uses to store the integer value.
    /// </summary>
    public int Size { get; set; } = 0;

    #region Ctors

    public VarInt(ulong value)
    {
        Value = value;
        Bytes = this.ULongToByteArray(value);
        Size = Bytes.Length;
    }

    public VarInt(byte[] bytes)
    {
        Bytes = bytes;
        int index = 0;
        ulong value = 0;
        byte b;
        do
        {
            value = (value << 7) | (ulong)((b = bytes[index]) & (ulong)0x7F);
            index++;
        } while ((b & 0x80) != 0);

        Size = index;
        Value = value;
        Bytes = new byte[Size];
        Array.Copy(bytes, 0, Bytes, 0, Size);
    }

    #endregion

    #region Implicit Ctor

    // Int8 / sbyte
    public static implicit operator sbyte(VarInt value)
    {
        return (sbyte)value.Value;
    }

    // Int8 / sbyte
    public static implicit operator VarInt(sbyte value)
    {
        return new VarInt((ulong)value);
    }

    // UInt8 / byte
    public static implicit operator byte(VarInt value)
    {
        return (byte)value.Value;
    }

    // UInt8 / byte
    public static implicit operator VarInt(byte value)
    {
        return new VarInt(value);
    }

    // Int16
    public static implicit operator Int16(VarInt value)
    {
        return (Int16)value.Value;
    }

    // Int16
    public static implicit operator VarInt(Int16 value)
    {
        return new VarInt((ulong)value);
    }

    // UInt16
    public static implicit operator UInt16(VarInt value)
    {
        return (UInt16)value.Value;
    }

    // UInt16
    public static implicit operator VarInt(UInt16 value)
    {
        return new VarInt(value);
    }

    // Int32
    public static implicit operator Int32(VarInt value)
    {
        return (Int32)value.Value;
    }

    // Int32
    public static implicit operator VarInt(Int32 value)
    {
        return new VarInt((ulong)value);
    }

    // Int32
    public static implicit operator UInt32(VarInt value)
    {
        return (UInt32)value.Value;
    }

    // Int32
    public static implicit operator VarInt(UInt32 value)
    {
        return new VarInt(value);
    }

    // Int64
    public static implicit operator Int64(VarInt value)
    {
        return (Int64)value.Value;
    }

    // Int64
    public static implicit operator VarInt(Int64 value)
    {
        return new VarInt((ulong)value);
    }

    // UInt64
    public static implicit operator UInt64(VarInt value)
    {
        return (UInt64)value.Value;
    }

    // UInt64
    public static implicit operator VarInt(UInt64 value)
    {
        return new VarInt(value);
    }

    #endregion

    #region Operator Overloading

    public static VarInt operator +(VarInt left, VarInt right)
    {
        return new VarInt(left + right);
    }

    public static VarInt operator -(VarInt left, VarInt right)
    {
        return new VarInt(left - right);
    }

    public static VarInt operator *(VarInt left, VarInt right)
    {
        return new VarInt(left * right);
    }

    public static VarInt operator /(VarInt left, VarInt right)
    {
        return new VarInt(left / right);
    }

    #endregion

    #region Private members

    private byte[] ULongToByteArray(ulong value)
    {
        byte[] bytes = new byte[9]; // max length for Int64 -> VarInt is 9 bytes
        int index = 0;

        byte buffer = (byte)(value & 0x7F);
        bytes[index] = buffer;

        while ((value >>= 7) > 0)
        {
            index++;
            buffer = (byte)(value & 0x7F | 0x80);   // 0x80 is continuation bit

            bytes[index] = buffer;
        }

        Array.Reverse(bytes, 0, index + 1);
        return bytes[0..(index + 1)];
    }

    #endregion
}