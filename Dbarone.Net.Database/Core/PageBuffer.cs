namespace Dbarone.Net.Database;
using System.Text;

/// <summary>
/// Represents an 8K page in the database. This is the smallest unit written / read to / from disk.
/// </summary>
public class PageBuffer : BufferBase, IBuffer
{
    private int _pageId;
    private uint _pageSize = 8192;

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
}