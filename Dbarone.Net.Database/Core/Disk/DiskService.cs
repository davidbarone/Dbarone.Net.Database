namespace Dbarone.Net.Database;

/// <summary>
/// Services for reading / writing to disk
/// </summary>
public class DiskService
{
    private Stream _stream;
    private int _pageCount;

    public DiskService(Stream stream)
    {
        this._stream = stream;
        this._pageCount = (int)this._stream.Length / 8192;
    }

    /// <summary>
    /// Returns the number of pages in the database.
    /// </summary>
    public int PageCount {get { return this._pageCount; } }

    /// <summary>
    /// Creates and persists a new page to disk.
    /// </summary>
    /// <param name="pageType"></param>
    /// <returns></returns>
    public int CreatePage(PageType pageType)
    {
        int pageSize = 8192;
        byte[] buffer = new byte[pageSize];
        int start = (_pageCount * pageSize);
        int length = pageSize;
        this._stream.Write(buffer, start, length);
        _pageCount++;
        return this._pageCount - 1; // zero-based
    }

    public PageBuffer ReadPage(int pageId)
    {
        byte[] buffer = new byte[8192];
        int start = (pageId * 8192);
        int length = 8192;
        int read = this._stream.Read(buffer, start, length);
        return new PageBuffer(buffer, pageId);
    }

    public void WritePage(PageBuffer page)
    {

    }

    /// <summary>
    /// Creates a new empty page not currently persisted to disk
    /// </summary>
    /// <returns></returns>
    public PageBuffer NewPage()
    {
        var buffer = new byte[8192];
        return new PageBuffer(buffer, -1);
    }
}