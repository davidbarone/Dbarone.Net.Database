namespace Dbarone.Net.Database;
using System.Text;

/// <summary>
/// Represents an 8K page in the database. This is the smallest unit written / read to / from disk.
/// </summary>
public class PageBuffer
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