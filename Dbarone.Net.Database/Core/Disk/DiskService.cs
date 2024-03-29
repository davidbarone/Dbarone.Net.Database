namespace Dbarone.Net.Database;
using Dbarone.Net.Assertions;

/// <summary>
/// Services for reading / writing to disk
/// </summary>
public class DiskService : IDiskService
{
    private Stream _stream;
    private int _pageCount;

    public DiskService(Stream stream)
    {
        this._stream = stream;
        this._pageCount = (int)(this._stream.Length / Global.PageSize);
    }

    /// <summary>
    /// Returns the number of pages in the database.
    /// </summary>
    public int PageCount { get { return this._pageCount; } }

    /// <summary>
    /// Creates storage on disk for a new page.
    /// </summary>
    /// <returns></returns>
    public int CreatePage()
    {
        int pageSize = Global.PageSize;
        byte[] buffer = new byte[pageSize];
        int start = (_pageCount * pageSize);
        int length = pageSize;
        this._stream.Position = start;
        this._stream.Write(buffer, 0, length);
        _pageCount++;
        return this._pageCount - 1; // zero-based
    }

    /// <summary>
    /// Clears all data on a page. Used for repurposing free pages.
    /// </summary>
    public void ClearPage(int pageId) {
        int pageSize = Global.PageSize;
        byte[] buffer = new byte[pageSize];
        int start = (pageId * pageSize);
        int length = pageSize;
        this._stream.Position = start;
        this._stream.Write(buffer, 0, length);
    }

    public PageBuffer ReadPage(int pageId)
    {
        byte[] buffer = new byte[Global.PageSize];
        int start = (pageId * Global.PageSize);
        int length = Global.PageSize;
        this._stream.Position = start;
        int read = this._stream.Read(buffer, 0, length);
        return new PageBuffer(buffer, pageId);
    }

    public void WritePage(int pageId, PageBuffer page)
    {
        var buffer = page.ToArray();
        Assert.Equals(buffer.Length, Global.PageSize);
        var start = (pageId * Global.PageSize);
        this._stream.Position = start;
        this._stream.Write(buffer);
    }

}