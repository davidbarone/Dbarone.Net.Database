namespace Dbarone.Net.Database;
using Dbarone.Net.Assertions;

/// <summary>
/// In memory cache of pages as they are modified or read from disk. Dirty pages are written back to disk with a CHECKPOINT command.
/// </summary>
public class BufferManager
{
    public BufferManager(DiskService diskService)
    {
        this._diskService = diskService;
    }

    private DiskService _diskService;
    private Dictionary<int, Page<PageHeader, PageData>> _pages = new Dictionary<int, Page<PageHeader, PageData>>();

    /// <summary>
    /// Gets a page from the buffer cache. If page not present, reads from disk
    /// </summary>
    /// <param name="pageId"></param>
    /// <returns></returns>
    public T GetPage<T>(int pageId) where T : Page<PageHeader, PageData>
    {
        if (_pages.ContainsKey(pageId))
        {
            // cache hit
            var page = _pages[pageId];
            Assert.AssignableFrom(page, typeof(Page<PageHeader, PageData>));
            return (T)_pages[pageId];
        }
        else
        {
            // cache miss - read from disk + add to buffer cache
            var buffer = _diskService.ReadPage(pageId);
            T page = default!;
            if (typeof(T) == typeof(BootPage)) page = (T)(object)new BootPage(pageId, buffer);
            else if (typeof(T) == typeof(SystemTablePage)) page = (T)(object)new SystemTablePage(pageId, buffer);
            else if (typeof(T) == typeof(SystemColumnPage)) page = (T)(object)new SystemColumnPage(pageId, buffer);
            else throw new Exception("Unable to create a new page.");
            this._pages[pageId] = page;
            return page;
        }
    }

    public int CreatePage(PageType pageType)
    {
        var pageId = _diskService.CreatePage(pageType);
        return pageId;
    }
}