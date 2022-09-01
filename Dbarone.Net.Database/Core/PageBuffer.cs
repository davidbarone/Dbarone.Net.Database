namespace Dbarone.Net.Database;
using System.Text;

/// <summary>
/// Represents an 8K page in the database. This is the smallest unit written / read to / from disk.
/// </summary>
public class PageBuffer : BufferBase, IBuffer
{
    private int _pageId;

    protected override byte[] InternalBuffer
    {
        get
        {
            return _buffer;
        }
    }

    public PageBuffer(byte[] buffer, int pageId) : base(buffer)
    {
        this._pageId = pageId;
    }

    public override byte[] ToArray()
    {
        // copy existing buffer
        var buffer = new byte[_buffer.Length];
        Buffer.BlockCopy(_buffer, 0, buffer, 0, _buffer.Length);
        return buffer;
    }

    /// <summary>
    /// Returns whether the page buffer actually contains real data.
    /// The first 6 bytes contain 3 UInt16 values:
    /// - Column Count
    /// - Buffer Length
    /// - Data Length
    /// 
    /// If these are all zero, then safe to assume the page buffer is empty.
    /// </summary>
    /// <returns></returns>
    public bool IsEmpty()
    {
        return this[0] == 0 && this[1] == 0 && this[2] == 0 && this[3] == 0 && this[4] == 0 && this[5] == 0;
    }
}