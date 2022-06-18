namespace Dbarone.Net.Database;
using System.Text;

/// <summary>
/// Represents an 8K page in the database. This is the smallest unit written / read to / from disk.
/// </summary>
public class PageBuffer : IPageBuffer
{
    private byte[] _buffer;
    MemoryStream _stream;
    private int _pageId;
    private uint _pageSize = 8192;

    public PageBuffer(byte[] buffer, int pageId)
    {
        this._buffer = buffer;
        this._stream = new MemoryStream(buffer);    // for writing to the byte array without using pointers / unsafe code.
        this._pageId = pageId;
    }

    public byte this[int index]
    {
        get => this._buffer[(this._pageId * this._pageSize) + index];
        set => this._buffer[(this._pageId * this._pageSize) + index] = value;
    }

    public void Clear(int index, int length)
    {
        System.Array.Clear(_buffer, index, length);
    }

    public void Fill(int index, int length, byte value)
    {
        for (var i = 0; i < length; i++)
        {
            _buffer[index + i] = value;
        }
    }

    public byte[] ToArray()
    {
        // copy existing buffer
        var buffer = new byte[_buffer.Length];
        Buffer.BlockCopy(_buffer, 0, buffer, 0, _buffer.Length);
        return buffer;
    }

    #region Read methods

    public bool ReadBool(int index)
    {
        return _buffer[index] != 0;
    }

    public byte ReadByte(int index)
    {
        return _buffer[index];
    }

    public Int16 ReadInt16(int index)
    {
        return BitConverter.ToInt16(_buffer, index);
    }

    public UInt16 ReadUInt16(int index)
    {
        return BitConverter.ToUInt16(_buffer, index);
    }

    public Int32 ReadInt32(int index)
    {
        return BitConverter.ToInt32(_buffer, index);
    }

    public UInt32 ReadUInt32(int index)
    {
        return BitConverter.ToUInt32(_buffer, index);
    }

    public Int64 ReadInt64(int index)
    {
        return BitConverter.ToInt64(_buffer, index);
    }

    public UInt64 ReadUInt64(int index)
    {
        return BitConverter.ToUInt64(_buffer, index);
    }

    public Double ReadDouble(int index)
    {
        return BitConverter.ToDouble(_buffer, index);
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
        Buffer.BlockCopy(_buffer, index, bytes, 0, length);
        return bytes;
    }

    public DateTime ReadDateTime(int index)
    {
        return DateTime.FromBinary(this.ReadInt64(index));
    }

    public string ReadString(int index, int length)
    {
        return Encoding.UTF8.GetString(_buffer, index, length);
    }

    #endregion

    #region Write methods

    public void Write(bool value, int index)
    {
        var bytes = BitConverter.GetBytes(value);
        this._stream.Write(bytes, index, bytes.Length);
    }

    public void Write(byte value, int index)
    {
        var bytes = BitConverter.GetBytes(value);
        this._stream.Write(bytes, index, bytes.Length);
    }

    public void Write(Int16 value, int index)
    {
        var bytes = BitConverter.GetBytes(value);
        this._stream.Write(bytes, index, bytes.Length);
    }

    public void Write(UInt16 value, int index)
    {
        var bytes = BitConverter.GetBytes(value);
        this._stream.Write(bytes, index, bytes.Length);
    }

    public void Write(Int32 value, int index)
    {
        var bytes = BitConverter.GetBytes(value);
        this._stream.Write(bytes, index, bytes.Length);
    }

    public void Write(UInt32 value, int index)
    {
        var bytes = BitConverter.GetBytes(value);
        this._stream.Write(bytes, index, bytes.Length);
    }

    public void Write(Int64 value, int index)
    {
        var bytes = BitConverter.GetBytes(value);
        this._stream.Write(bytes, index, bytes.Length);
    }

    public void Write(UInt64 value, int index)
    {
        var bytes = BitConverter.GetBytes(value);
        this._stream.Write(bytes, index, bytes.Length);
    }

    public void Write(Double value, int index)
    {
        var bytes = BitConverter.GetBytes(value);
        this._stream.Write(bytes, index, bytes.Length);
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
        Buffer.BlockCopy(value, 0, this._buffer, index, value.Length);
    }

    public void Write(DateTime value, int index)
    {
        this.Write(value.ToBinary(), index);
    }

    public void Write(string value, int index)
    {
        // GetBytes writes directly to the buffer.
        var bytes = Encoding.UTF8.GetBytes(value, 0, value.Length, this._buffer, index);
    }

    #endregion
}