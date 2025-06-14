using System.Text;

namespace Dbarone.Net.Database;

/// <summary>
/// Represents a generic memory buffer.
/// </summary>
public class PageBuffer : IBuffer
{
    protected MemoryStream Stream;
    protected byte[] _buffer;

    public long Position
    {
        get
        {
            return (int)this.Stream.Position;
        }
        set
        {
            this.Stream.Position = value;
        }
    }

    /// <summary>
    /// Bytes (0,4) denote the page id.
    /// </summary>
    public int PageId
    {
        get
        {
            return this.ReadInt64(0);
        }
        set
        {
            this.Write((Int32)value, 0);
        }
    }

    /// <summary>
    /// Byte (4,1) denotes the page type.
    /// </summary>
    public PageType PageType
    {
        get
        {
            return (PageType)this.ReadByte(4);
        }
        set
        {
            this.Write((byte)value, 4);
        }
    }

    public PageBuffer(byte[] buffer)
    {
        // MemoryStream with fixed capacity buffer
        this._buffer = buffer;
        this.Stream = new MemoryStream(buffer);
    }

    /// <summary>
    /// The internal byte array used for read and write operations.
    /// </summary>
    protected virtual byte[] InternalBuffer { get { return this._buffer; } }

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

    public long Length => this.InternalBuffer.Length;

    #region Read methods

    public bool ReadBool()
    {
        return InternalBuffer[this.Position] != 0;
    }

    public VarInt ReadVarInt()
    {
        var index = this.Position;
        int i = 0;
        byte[] bytes = new byte[4];
        Byte b;
        do
        {
            b = InternalBuffer[index + i];
            bytes[i] = b;
            i++;
        } while ((b & 128) != 0);
        return new VarInt(bytes[0..i]);
    }

    public Int64 ReadInt64()
    {
        return BitConverter.ToInt64(InternalBuffer, (int)this.Position);
    }

    public Double ReadDouble()
    {
        return BitConverter.ToDouble(InternalBuffer, (int)this.Position);
    }

    public byte[] ReadBytes(int length)
    {
        var bytes = new byte[length];
        Buffer.BlockCopy(InternalBuffer, (int)this.Position, bytes, 0, length);
        return bytes;
    }

    public DateTime ReadDateTime()
    {
        return DateTime.FromBinary(this.ReadInt64());
    }

    public string ReadString(int length, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        if (textEncoding == TextEncoding.UTF8)
        {
            return Encoding.UTF8.GetString(InternalBuffer, (int)this.Position, length);
        }
        else if (textEncoding == TextEncoding.Latin1)
        {
            return Encoding.Latin1.GetString(InternalBuffer, (int)this.Position, length);
        }
        throw new Exception("Unable to read string encoding.");
    }

    public object Read(DocumentType documentType, int? length = null, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        switch (documentType)
        {
            case DocumentType.Boolean:
                return ReadBool();
            case DocumentType.Real:
                return ReadDouble();
            case DocumentType.Integer:
                return ReadInt64();
            case DocumentType.DateTime:
                return ReadDateTime();
            case DocumentType.Text:
                if (length == null) { throw new Exception("Length required (1)."); }
                return ReadString(length.Value, textEncoding);
            case DocumentType.Blob:
                if (length == null) { throw new Exception("Length required (2)."); }
                return ReadBytes(length.Value);
        }
        throw new Exception($"Invalid data type.");
    }

    #endregion

    #region Write methods

    public void Write(bool value)
    {
        var bytes = BitConverter.GetBytes(value);
        this.Stream.Write(bytes, 0, bytes.Length);
    }

    public void Write(Int64 value)
    {
        var bytes = BitConverter.GetBytes(value);
        this.Stream.Write(bytes, 0, bytes.Length);
    }

    public void Write(Double value)
    {
        var bytes = BitConverter.GetBytes(value);
        this.Stream.Write(bytes, 0, bytes.Length);
    }

    public void Write(DateTime value)
    {
        this.Write(value.ToBinary());
    }

    public void Write(byte[] value)
    {
        Buffer.BlockCopy(value, 0, this.InternalBuffer, (int)this.Position, value.Length);
    }

    public void Write(string value, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        if (textEncoding == TextEncoding.UTF8)
        {
            // GetBytes writes directly to the buffer.
            var bytes = Encoding.UTF8.GetBytes(value, 0, value.Length, this.InternalBuffer, (int)this.Position);
        }
        else if (textEncoding == TextEncoding.Latin1)
        {
            var bytes = Encoding.Latin1.GetBytes(value, 0, value.Length, this.InternalBuffer, (int)this.Position);
        }
        else
        {
            throw new Exception("Unable to write string encoding.");
        }
    }

    public void Write(object value, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        var type = value.GetType();
        if (type.IsEnum)
        {
            type = Enum.GetUnderlyingType(type);
        }
        if (type == typeof(bool))
        {
            Write((bool)value);
        }
        else if (type == typeof(Int64))
        {
            Write((Int64)value);
        }
        else if (type == typeof(double))
        {
            Write((double)value);
        }
        else if (type == typeof(DateTime))
        {
            Write((DateTime)value);
        }
        else if (type == typeof(string))
        {
            Write((string)value, textEncoding);
        }
        else if (type == typeof(byte[]))
        {
            Write((byte[])value);
        }
        else
        {
            throw new Exception($"Type {type.Name} is not supported for buffer writing.");
        }
    }

    #endregion
}