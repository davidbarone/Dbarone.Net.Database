namespace Dbarone.Net.Database;

/// <summary>
/// Represents a variable-length unsigned integer using base-128 representation.
/// https://en.wikipedia.org/wiki/Variable-length_quantity
/// </summary>
public struct VarInt
{
    public int Value { get; set; } = 0;
    public byte[] Bytes { get; set; } = new byte[0];
    public int Length { get; set; } = 0;

    private byte[] IntToByteArray(int value)
    {
        byte[] bytes = new byte[4];
        int index = 0;
        int buffer = value & 0x7F;

        while ((value >>= 7) > 0)
        {
            buffer <<= 8;
            buffer |= 0x80;
            buffer += (value & 0x7F);
        }
        while (true)
        {
            bytes[index] = (byte)buffer;
            index++;
            if ((buffer & 0x80) > 0)
                buffer >>= 8;
            else
                break;
        }

        return bytes[0..index];
    }

    /// <summary>
    /// Adds an integer to the current VarInt. The method will return the overflow status of the operation (if the result of the operation increases the storage size).
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool Add(int value)
    {
        var startLength = Length;
        this.Value = this.Value + value;
        this.Bytes = IntToByteArray(this.Value);
        this.Length = this.Bytes.Length;
        if (Length != startLength)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public VarInt(int value)
    {
        Value = value;
        Bytes = this.IntToByteArray(value);
        Length = Bytes.Length;
    }

    public VarInt(byte[] bytes)
    {
        Bytes = bytes;
        int index = 0;
        int value = 0;
        byte b;
        do
        {
            value = (value << 7) | ((b = bytes[index]) & 0x7F);
            index++;
        } while ((b & 0x80) != 0);

        Length = index;
        Value = value;
        Bytes = new byte[Length];
        Array.Copy(bytes, 0, Bytes, 0, Length);
    }
}