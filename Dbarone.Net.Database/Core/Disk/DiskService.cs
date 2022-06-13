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
    /// Creates and persists a new page to disk.
    /// </summary>
    /// <param name="pageType"></param>
    /// <returns></returns>
    public PageBuffer CreatePage(PageType pageType){
        byte[] buffer = new byte[8192];
        int start = (_pageCount * 8192);
        int length = 8192;
        this._stream.Write(buffer, start, length);
        return null;

    }

    public PageBuffer ReadPage(int pageId)
    {
        byte[] buffer = new byte[8192];
        int start = (pageId * 8192);
        int length = 8192;
        int read = this._stream.Read(buffer, start, length);
        return null;
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