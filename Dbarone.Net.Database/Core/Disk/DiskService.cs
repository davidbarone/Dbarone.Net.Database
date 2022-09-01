namespace Dbarone.Net.Database;

/// <summary>
/// Services for reading / writing to disk
/// </summary>
public class DiskService
{
    private Stream _stream;
    private uint _pageCount;

    public DiskService(Stream stream)
    {
        this._stream = stream;
        this._pageCount = (uint)this._stream.Length / 8192;
    }

    /// <summary>
    /// Returns the number of pages in the database.
    /// </summary>
    public uint PageCount {get { return this._pageCount; } }

    /// <summary>
    /// Creates and persists a new page to disk.
    /// </summary>
    /// <param name="pageType"></param>
    /// <returns></returns>
    public uint CreatePage(PageType pageType)
    {
        uint pageSize = 8192;
        byte[] buffer = new byte[pageSize];
        uint start = (_pageCount * pageSize);
        uint length = pageSize;
        this._stream.Position = start;
        this._stream.Write(buffer, 0, (int)length);
        _pageCount++;
        return (uint)this._pageCount - 1; // zero-based
    }

    public PageBuffer ReadPage(uint pageId)
    {
        byte[] buffer = new byte[8192];
        uint start = (pageId * 8192);
        int length = 8192;
        this._stream.Position = start;
        int read = this._stream.Read(buffer, 0, length);
        return new PageBuffer(buffer, pageId);
    }

    public void WritePage(PageBuffer page)
    {

    }
}