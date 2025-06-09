using System.Text;

namespace Dbarone.Net.Database;

/// <summary>
/// Represents a generic memory buffer.
/// </summary>
public class GenericBuffer : IBuffer
{
    private byte[] buffer;

    protected MemoryStream Stream;

    /// <summary>
    /// Returns true if the current buffer can grow.
    /// </summary>
    public bool Resizeable { get; private set; }

    /// <summary>
    /// Gets / sets the position in the buffer.
    /// </summary>
    public long Position
    {
        get { return this.Stream.Position; }
        set { this.Stream.Position = value; }
    }

    /// <summary>
    /// Returns the length of the underlying buffer in bytes.
    /// </summary>
    public long Length
    {
        get { return this.Stream.Length; }
    }

    /// <summary>
    /// Creates a non-resizeable buffer.
    /// </summary>
    /// <param name="buffer"></param>
    public GenericBuffer(byte[] buffer)
    {
        // MemoryStream with fixed capacity buffer
        this.buffer = buffer;
        this.Stream = new MemoryStream(buffer);
        this.Resizeable = false;
    }

    /// <summary>
    /// Creates an expandable buffer.
    /// </summary>
    public GenericBuffer()
    {
        this.Stream = new MemoryStream();
        this.Resizeable = true;
    }

    /// <summary>
    /// The internal byte array used for read and write operations. For resizeable buffers
    /// the buffer returned is the underlying MemoryStream buffer, and may return more
    /// bytes than actually populated. You will need to use the MemoryStream.Length property
    /// to get the actual size of the buffer.
    /// </summary>
    protected virtual byte[] InternalBuffer
    {
        get
        {
            return this.Resizeable ? this.Stream.GetBuffer() : this.buffer;
        }
    }

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
        var buffer = new byte[this.Length];
        Buffer.BlockCopy(InternalBuffer, 0, buffer, 0, (int)this.Length);
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

    public bool ReadBool()
    {
        var index = (int)this.Stream.Position;
        var result = InternalBuffer[index] != 0;
        this.Position += sizeof(Boolean);
        return result;
    }

    public byte ReadByte()
    {
        var index = (int)this.Stream.Position;
        var result = InternalBuffer[index];
        this.Position += sizeof(Byte);
        return result;
    }

    public sbyte ReadSByte()
    {
        var index = (int)this.Stream.Position;
        var result = (sbyte)InternalBuffer[index];
        this.Position += sizeof(SByte);
        return result;
    }

    public char ReadChar()
    {
        var index = (int)this.Stream.Position;
        var result = BitConverter.ToChar(InternalBuffer, index);
        this.Position += sizeof(Char);
        return result;
    }

    public VarInt ReadVarInt()
    {
        int start = (int)this.Position;
        byte[] bytes = new byte[4];
        Byte b;
        do
        {
            b = InternalBuffer[(int)this.Position];
            bytes[(int)this.Position - start] = b;
            this.Position++;
        } while ((b & 128) != 0);
        return new VarInt(bytes[0..((int)this.Position - start)]);
    }

    public Int16 ReadInt16()
    {
        var index = (int)this.Stream.Position;
        var result = BitConverter.ToInt16(InternalBuffer, index);
        this.Position += sizeof(Int16);
        return result;
    }

    public UInt16 ReadUInt16()
    {
        var index = (int)this.Stream.Position;
        var result = BitConverter.ToUInt16(InternalBuffer, index);
        this.Position += sizeof(UInt16);
        return result;

    }

    public Int32 ReadInt32()
    {
        var index = (int)this.Stream.Position;
        var result = BitConverter.ToInt32(InternalBuffer, index);
        this.Position += sizeof(Int32);
        return result;
    }

    public UInt32 ReadUInt32()
    {
        var index = (int)this.Stream.Position;
        var result = BitConverter.ToUInt32(InternalBuffer, index);
        this.Position += sizeof(UInt32);
        return result;
    }

    public Int64 ReadInt64()
    {
        var index = (int)this.Stream.Position;
        var result = BitConverter.ToInt64(InternalBuffer, index);
        this.Position += sizeof(Int64);
        return result;
    }

    public UInt64 ReadUInt64()
    {
        var index = (int)this.Stream.Position;
        var result = BitConverter.ToUInt64(InternalBuffer, index);
        this.Position += sizeof(UInt64);
        return result;
    }

    public Double ReadDouble()
    {
        var index = (int)this.Stream.Position;
        var result = BitConverter.ToDouble(InternalBuffer, index);
        this.Position += sizeof(Double);
        return result;
    }

    public Single ReadSingle()
    {
        var index = (int)this.Stream.Position;
        var result = BitConverter.ToSingle(InternalBuffer, index);
        this.Position += sizeof(Single);
        return result;
    }

    public Decimal ReadDecimal()
    {
        var a = this.ReadInt32();
        var b = this.ReadInt32();
        var c = this.ReadInt32();
        var d = this.ReadInt32();
        return new Decimal(new int[] { a, b, c, d });
    }

    public Guid ReadGuid()
    {
        var index = (int)this.Stream.Position;
        var result = new Guid(this.ReadBytes(16));
        this.Position += 16;
        return result;
    }

    public byte[] ReadBytes(int length)
    {
        var index = (int)this.Stream.Position;
        var bytes = new byte[length];
        Buffer.BlockCopy(InternalBuffer, index, bytes, 0, length);
        this.Position += length;
        return bytes;
    }

    public DateTime ReadDateTime()
    {
        var index = (int)this.Stream.Position;
        var result = DateTime.FromBinary(this.ReadInt64());
        return result;
    }

    public string ReadString(int length, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        var index = (int)this.Stream.Position;
        if (textEncoding == TextEncoding.UTF8)
        {
            var result = Encoding.UTF8.GetString(InternalBuffer, index, length);
            this.Position += length;
            return result;
        }
        else if (textEncoding == TextEncoding.UTF16)
        {
            var result = Encoding.Unicode.GetString(InternalBuffer, index, length);
            this.Position += length;
            return result;
        }
        else if (textEncoding == TextEncoding.Latin1)
        {
            var result = Encoding.Latin1.GetString(InternalBuffer, index, length);
            this.Position += length;
            return result;
        }
        throw new Exception("Unable to read string encoding.");
    }

    public object Read(DocumentType dataType, int? length = null, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        switch (dataType)
        {
            case DocumentType.Boolean:
                return ReadBool();
            case DocumentType.Byte:
                return ReadByte();
            case DocumentType.SByte:
                return ReadSByte();
            case DocumentType.Char:
                return ReadChar();
            case DocumentType.Decimal:
                return ReadDecimal();
            case DocumentType.Double:
                return ReadDouble();
            case DocumentType.Single:
                return ReadSingle();
            case DocumentType.VarInt:
                return ReadVarInt();
            case DocumentType.Int16:
                return ReadInt16();
            case DocumentType.UInt16:
                return ReadUInt16();
            case DocumentType.Int32:
                return ReadInt32();
            case DocumentType.UInt32:
                return ReadUInt32();
            case DocumentType.Int64:
                return ReadInt64();
            case DocumentType.UInt64:
                return ReadUInt64();
            case DocumentType.DateTime:
                return ReadDateTime();
            case DocumentType.String:
                if (length == null) { throw new Exception("Length required (1)."); }
                return ReadString(length.Value, textEncoding);
            case DocumentType.Guid:
                return ReadGuid();
            case DocumentType.Blob:
                if (length == null) { throw new Exception("Length required (2)."); }
                return ReadBytes(length.Value);
        }
        throw new Exception($"Invalid data type.");
    }

    #endregion

    #region Write methods

    public int Write(bool value)
    {
        var bytes = BitConverter.GetBytes(value);
        this.Stream.Write(bytes, 0, bytes.Length);
        return bytes.Length;
    }

    public int Write(byte value)
    {
        // BitConverter.GetBytes(Byte) treats the byte value as a ushort, so returns 2 bytes.
        // Section 6.1.2 of the C# language spec
        // Take 1st byte only.
        var bytes = BitConverter.GetBytes((ushort)value).Take(1).ToArray();
        this.Stream.Write(bytes, 0, bytes.Length);
        return bytes.Length;
    }

    public int Write(sbyte value)
    {
        // BitConverter.GetBytes(SByte) treats the sbyte value as a short, so returns 2 bytes.
        // Section 6.1.2 of the C# language spec
        // Take 1st byte only.
        var bytes = BitConverter.GetBytes((short)value).Take(1).ToArray();
        this.Stream.Write(bytes, 0, bytes.Length);
        return bytes.Length;
    }

    public int Write(char value)
    {
        var bytes = BitConverter.GetBytes(value);
        this.Stream.Write(bytes, 0, bytes.Length);
        return bytes.Length;
    }

    public int Write(VarInt value)
    {
        this.Stream.Write(value.Bytes, 0, value.Size);
        return value.Size;
    }

    public int Write(Int16 value)
    {
        var bytes = BitConverter.GetBytes(value);
        this.Stream.Write(bytes, 0, bytes.Length);
        return bytes.Length;
    }

    public int Write(UInt16 value)
    {
        var bytes = BitConverter.GetBytes(value);
        this.Stream.Write(bytes, 0, bytes.Length);
        return bytes.Length;
    }

    public int Write(Int32 value)
    {
        var bytes = BitConverter.GetBytes(value);
        this.Stream.Write(bytes, 0, bytes.Length);
        return bytes.Length;
    }

    public int Write(UInt32 value)
    {
        var bytes = BitConverter.GetBytes(value);
        this.Stream.Write(bytes, 0, bytes.Length);
        return bytes.Length;
    }

    public int Write(Int64 value)
    {
        var bytes = BitConverter.GetBytes(value);
        this.Stream.Write(bytes, 0, bytes.Length);
        return bytes.Length;
    }

    public int Write(UInt64 value)
    {
        var bytes = BitConverter.GetBytes(value);
        this.Stream.Write(bytes, 0, bytes.Length);
        return bytes.Length;
    }

    public int Write(Double value)
    {
        var bytes = BitConverter.GetBytes(value);
        this.Stream.Write(bytes, 0, bytes.Length);
        return bytes.Length;
    }

    public int Write(Single value)
    {
        var bytes = BitConverter.GetBytes(value);
        this.Stream.Write(bytes, 0, bytes.Length);
        return bytes.Length;
    }

    public int Write(Decimal value)
    {
        // Split Decimal into 4 ints
        var bits = Decimal.GetBits(value);
        this.Write(bits[0]);
        this.Write(bits[1]);
        this.Write(bits[2]);
        this.Write(bits[3]);
        return bits.Length;
    }

    public int Write(Guid value)
    {
        var arr = value.ToByteArray();
        this.Write(arr);
        return arr.Length;
    }

    public int Write(byte[] value)
    {
        //var index = (int)this.Stream.Position;
        //Buffer.BlockCopy(value, 0, this.InternalBuffer, index, value.Length);
        this.Stream.Write(value, 0, value.Length);
        return value.Length;
    }

    public int Write(DateTime value)
    {
        var bin = value.ToBinary();
        return this.Write(bin);
    }

    public int Write(string value, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        var index = (int)this.Stream.Position;
        if (textEncoding == TextEncoding.UTF8)
        {
            // GetBytes writes directly to the buffer.
            //var bytes = Encoding.UTF8.GetBytes(value, 0, value.Length, this.InternalBuffer, index);

            var bytes = Encoding.UTF8.GetBytes(value);
            Write(bytes);
            return bytes.Length;
        }
        else if (textEncoding == TextEncoding.UTF16)
        {
            // GetBytes writes directly to the buffer.
            //var bytes = Encoding.UTF8.GetBytes(value, 0, value.Length, this.InternalBuffer, index);

            var bytes = Encoding.Unicode.GetBytes(value);
            Write(bytes);
            return bytes.Length;
        }
        else if (textEncoding == TextEncoding.Latin1)
        {
            //var bytes = Encoding.Latin1.GetBytes(value, 0, value.Length, this.InternalBuffer, index);

            var bytes = Encoding.Latin1.GetBytes(value);
            Write(bytes);
            return bytes.Length;
        }
        else
        {
            throw new Exception("Unable to write string encoding.");
        }
    }

    public int Write(object value, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        var type = value.GetType();
        if (type.IsEnum)
        {
            type = Enum.GetUnderlyingType(type);
        }

        if (type == typeof(bool))
        {
            return Write((bool)value);
        }
        else if (type == typeof(byte))
        {
            return Write((byte)value);
        }
        else if (type == typeof(sbyte))
        {
            return Write((sbyte)value);
        }
        else if (type == typeof(char))
        {
            return Write((char)value);
        }
        else if (type == typeof(decimal))
        {
            return Write((decimal)value);
        }
        else if (type == typeof(double))
        {
            return Write((double)value);
        }
        else if (type == typeof(Single))
        {
            return Write((Single)value);
        }
        else if (type == typeof(VarInt))
        {
            return Write((VarInt)value);
        }
        else if (type == typeof(Int16))
        {
            return Write((Int16)value);
        }
        else if (type == typeof(UInt16))
        {
            return Write((UInt16)value);
        }
        else if (type == typeof(Int32))
        {
            return Write((Int32)value);
        }
        else if (type == typeof(UInt32))
        {
            return Write((UInt32)value);
        }
        else if (type == typeof(Int64))
        {
            return Write((Int64)value);
        }
        else if (type == typeof(UInt64))
        {
            return Write((UInt64)value);
        }
        else if (type == typeof(DateTime))
        {
            return Write((DateTime)value);
        }
        else if (type == typeof(string))
        {
            return Write((string)value, textEncoding);
        }
        else if (type == typeof(Guid))
        {
            return Write((Guid)value);
        }
        else if (type == typeof(byte[]))
        {
            return Write((byte[])value);
        }
        else if (type == typeof(DateTime))
        {
            return Write((DateTime)value);
        }
        throw new Exception("Shouldn't get here!");
    }

    #endregion
}