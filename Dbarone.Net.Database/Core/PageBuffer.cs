namespace Dbarone.Net.Database;

/// <summary>
/// Represents an 8K page in the database. This is the smallest unit written / read to / from disk.
/// </summary>
public class PageBuffer
{
    private byte[] _buffer;
    private int _pageId;
    private uint _pageSize = 8192;

    public PageBuffer(byte[] buffer, int pageId)
    {
        this._buffer = buffer;
        this._pageId = pageId;
    }

    public byte this[int index]
    {
        get => this._buffer[(this._pageId * this._pageSize) + index];
        set => this._buffer[(this._pageId * this._pageSize) + index] = value;
    }
}