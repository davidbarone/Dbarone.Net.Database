namespace Dbarone.Net.Database;
using System.Text;

/// <summary>
/// Represents a generic memory buffer. The buffer auto grows as data is written past EOF.
/// </summary>
public class BufferBase : IBuffer
{
    protected MemoryStream Stream;

    public BufferBase()
    {
        // MemoryStream with autogrowth
        this.Stream = new MemoryStream();    // for writing to the byte array without using pointers / unsafe code.
    }

    /// <summary>
    /// The internal byte array used for read and write operations.
    /// </summary>
    protected virtual byte[] InternalBuffer { get { return Stream.GetBuffer(); } }

    public virtual byte this[int index]
    {
        get => this.InternalBuffer[index];
        set => this.InternalBuffer[index] = value;
    }

    public void Clear(int index, int length)
    {
        System.Array.Clear(InternalBuffer, index, length);
    }

    public void Fill(int index, int length, byte value)
    {
        for (var i = 0; i < length; i++)
        {
            InternalBuffer[index + i] = value;
        }
    }

    public virtual byte[] ToArray()
    {
        // copy existing buffer
        var buffer = new byte[InternalBuffer.Length];
        Buffer.BlockCopy(InternalBuffer, 0, buffer, 0, InternalBuffer.Length);
        return buffer;
    }

    public virtual byte[] Slice(int index, int length)
    {
        // copy existing buffer
        var buffer = new byte[length];
        Buffer.BlockCopy(InternalBuffer, index, buffer, 0, length);
        return buffer;
    }



    #region Read methods

    public bool ReadBool(int index)
    {
        return InternalBuffer[index] != 0;
    }

    public byte ReadByte(int index)
    {
        return InternalBuffer[index];
    }

    public sbyte ReadSByte(int index)
    {
        return (sbyte)InternalBuffer[index];
    }

    public char ReadChar(int index)
    {
        return (char)InternalBuffer[index];
    }

    public Int16 ReadInt16(int index)
    {
        return BitConverter.ToInt16(InternalBuffer, index);
    }

    public UInt16 ReadUInt16(int index)
    {
        return BitConverter.ToUInt16(InternalBuffer, index);
    }

    public Int32 ReadInt32(int index)
    {
        return BitConverter.ToInt32(InternalBuffer, index);
    }

    public UInt32 ReadUInt32(int index)
    {
        return BitConverter.ToUInt32(InternalBuffer, index);
    }

    public Int64 ReadInt64(int index)
    {
        return BitConverter.ToInt64(InternalBuffer, index);
    }

    public UInt64 ReadUInt64(int index)
    {
        return BitConverter.ToUInt64(InternalBuffer, index);
    }

    public Double ReadDouble(int index)
    {
        return BitConverter.ToDouble(InternalBuffer, index);
    }

    public Single ReadSingle(int index)
    {
        return BitConverter.ToSingle(InternalBuffer, index);
    }

    public Decimal ReadDecimal(int index)
    {
        var a = this.ReadInt32(index);
        var b = this.ReadInt32(index + 4);
        var c = this.ReadInt32(index + 8);
        var d = this.ReadInt32(index + 12);
        return new Decimal(new int[] { a, b, c, d });
    }

    public Guid ReadGuid(int index)
    {
        return new Guid(this.ReadBytes(index, 16));
    }

    public byte[] ReadBytes(int index, int length)
    {
        var bytes = new byte[length];
        Buffer.BlockCopy(InternalBuffer, index, bytes, 0, length);
        return bytes;
    }

    public DateTime ReadDateTime(int index)
    {
        return DateTime.FromBinary(this.ReadInt64(index));
    }

    public string ReadString(int index, int length)
    {
        return Encoding.UTF8.GetString(InternalBuffer, index, length);
    }

    #endregion

    #region Write methods

    public void Write(bool value, int index)
    {
        var bytes = BitConverter.GetBytes(value);
        this.Stream.Position = index;
        this.Stream.Write(bytes, 0, bytes.Length);
    }

    public void Write(byte value, int index)
    {
        var bytes = BitConverter.GetBytes(value);
        this.Stream.Position = index;
        this.Stream.Write(bytes, 0, bytes.Length);
    }

    public void Write(sbyte value, int index)
    {
        var bytes = BitConverter.GetBytes(value);
        this.Stream.Position = index;
        this.Stream.Write(bytes, 0, bytes.Length);
    }

    public void Write(char value, int index)
    {
        var bytes = BitConverter.GetBytes(value);
        this.Stream.Position = index;
        this.Stream.Write(bytes, 0, bytes.Length);
    }

    public void Write(Int16 value, int index)
    {
        var bytes = BitConverter.GetBytes(value);
        this.Stream.Position = index;
        this.Stream.Write(bytes, 0, bytes.Length);
    }

    public void Write(UInt16 value, int index)
    {
        var bytes = BitConverter.GetBytes(value);
        this.Stream.Position = index;
        this.Stream.Write(bytes, 0, bytes.Length);
    }

    public void Write(Int32 value, int index)
    {
        var bytes = BitConverter.GetBytes(value);
        this.Stream.Position = index;
        this.Stream.Write(bytes, 0, bytes.Length);
    }

    public void Write(UInt32 value, int index)
    {
        var bytes = BitConverter.GetBytes(value);
        this.Stream.Position = index;
        this.Stream.Write(bytes, 0, bytes.Length);
    }

    public void Write(Int64 value, int index)
    {
        var bytes = BitConverter.GetBytes(value);
        this.Stream.Position = index;
        this.Stream.Write(bytes, 0, bytes.Length);
    }

    public void Write(UInt64 value, int index)
    {
        var bytes = BitConverter.GetBytes(value);
        this.Stream.Position = index;
        this.Stream.Write(bytes, 0, bytes.Length);
    }

    public void Write(Double value, int index)
    {
        var bytes = BitConverter.GetBytes(value);
        this.Stream.Position = index;
        this.Stream.Write(bytes, 0, bytes.Length);
    }

    public void Write(Single value, int index)
    {
        var bytes = BitConverter.GetBytes(value);
        this.Stream.Position = index;
        this.Stream.Write(bytes, 0, bytes.Length);
    }

    public void Write(Decimal value, int index)
    {
        // Split Decimal into 4 ints
        var bits = Decimal.GetBits(value);
        this.Write(bits[0], index);
        this.Write(bits[1], index + 4);
        this.Write(bits[2], index + 8);
        this.Write(bits[3], index + 12);
    }

    public void Write(Guid value, int index)
    {
        this.Write(value.ToByteArray(), index);
    }

    public void Write(byte[] value, int index)
    {
        Buffer.BlockCopy(value, 0, this.InternalBuffer, index, value.Length);
    }

    public void Write(DateTime value, int index)
    {
        this.Write(value.ToBinary(), index);
    }

    public void Write(string value, int index)
    {
        // GetBytes writes directly to the buffer.
        var bytes = Encoding.UTF8.GetBytes(value, 0, value.Length, this.InternalBuffer, index);
    }

    public void Write(object value, int index)
    {
        var type = value.GetType();
        if (type == typeof(bool))
        {
            Write((bool)value, index);
        }
        else if (type == typeof(byte))
        {
            Write((byte)value, index);
        }
        else if (type == typeof(sbyte))
        {
            Write((sbyte)value, index);
        }
        else if (type == typeof(char))
        {
            Write((char)value, index);
        }
        else if (type == typeof(decimal))
        {
            Write((decimal)value, index);
        }
        else if (type == typeof(double))
        {
            Write((double)value, index);
        }
        else if (type == typeof(Single))
        {
            Write((Single)value, index);
        }
        else if (type == typeof(Int16))
        {
            Write((Int16)value, index);
        }
        else if (type == typeof(UInt16))
        {
            Write((UInt16)value, index);
        }
        else if (type == typeof(Int32))
        {
            Write((Int32)value, index);
        }
        else if (type == typeof(UInt32))
        {
            Write((UInt32)value, index);
        }
        else if (type == typeof(Int64))
        {
            Write((Int64)value, index);
        }
        else if (type == typeof(UInt64))
        {
            Write((UInt64)value, index);
        }
        else if (type == typeof(DateTime))
        {
            Write((DateTime)value, index);
        }
        else if (type == typeof(string))
        {
            Write((string)value, index);
        }
        else if (type == typeof(Guid))
        {
            Write((Guid)value, index);
        }
        else if (type == typeof(byte[]))
        {
            Write((byte[])value, index);
        }
        else if (type == typeof(DateTime))
        {
            Write((DateTime)value, index);
        }
    }

    #endregion
}