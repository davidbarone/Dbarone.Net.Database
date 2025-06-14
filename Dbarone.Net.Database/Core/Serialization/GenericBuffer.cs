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

    public Int64 ReadInt64()
    {
        var index = (int)this.Stream.Position;
        var result = BitConverter.ToInt64(InternalBuffer, index);
        this.Position += sizeof(Int64);
        return result;
    }

    public Double ReadDouble()
    {
        var index = (int)this.Stream.Position;
        var result = BitConverter.ToDouble(InternalBuffer, index);
        this.Position += sizeof(Double);
        return result;
    }

    public DateTime ReadDateTime()
    {
        var index = (int)this.Stream.Position;
        var result = DateTime.FromBinary(this.ReadInt64());
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
            case DocumentType.Integer:
                return ReadInt64();
            case DocumentType.Real:
                return ReadDouble();
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
        //return bytes.Length;
    }

    public void Write(Int64 value)
    {
        var bytes = BitConverter.GetBytes(value);
        this.Stream.Write(bytes, 0, bytes.Length);
        //return bytes.Length;
    }

    public void Write(Double value)
    {
        var bytes = BitConverter.GetBytes(value);
        this.Stream.Write(bytes, 0, bytes.Length);
        //return bytes.Length;
    }

    public void Write(DateTime value)
    {
        var bin = value.ToBinary();
        this.Write(bin);
    }

    public void Write(byte[] value)
    {
        //var index = (int)this.Stream.Position;
        //Buffer.BlockCopy(value, 0, this.InternalBuffer, index, value.Length);
        this.Stream.Write(value, 0, value.Length);
        //return value.Length;
    }

    public void Write(string value, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        var index = (int)this.Stream.Position;
        if (textEncoding == TextEncoding.UTF8)
        {
            // GetBytes writes directly to the buffer.
            //var bytes = Encoding.UTF8.GetBytes(value, 0, value.Length, this.InternalBuffer, index);

            var bytes = Encoding.UTF8.GetBytes(value);
            Write(bytes);
            //return bytes.Length;
        }
        else if (textEncoding == TextEncoding.UTF16)
        {
            // GetBytes writes directly to the buffer.
            //var bytes = Encoding.UTF8.GetBytes(value, 0, value.Length, this.InternalBuffer, index);

            var bytes = Encoding.Unicode.GetBytes(value);
            Write(bytes);
            //return bytes.Length;
        }
        else if (textEncoding == TextEncoding.Latin1)
        {
            //var bytes = Encoding.Latin1.GetBytes(value, 0, value.Length, this.InternalBuffer, index);

            var bytes = Encoding.Latin1.GetBytes(value);
            Write(bytes);
            //return bytes.Length;
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
        throw new Exception("Shouldn't get here!");
    }

    #endregion
}