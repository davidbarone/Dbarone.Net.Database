namespace Dbarone.Net.Database;
using Dbarone.Net.Assertions;

/// <summary>
/// In memory cache of pages as they are modified or read from disk. Dirty pages are written back to disk with a CHECKPOINT command.
/// </summary>
public class BufferManager
{
    public BufferManager(DiskService diskService){
        this._diskService = diskService;
    }

    private DiskService _diskService;
    private Dictionary<int, Page> _pages = new Dictionary<int, Page>();

    /// <summary>
    /// Gets a page from the buffer cache. If page not present, reads from disk
    /// </summary>
    /// <param name="pageId"></param>
    /// <returns></returns>
    public T GetPage<T>(int pageId) where T: Page {
        if (_pages.ContainsKey(pageId)) {
            // cache hit
            var page = _pages[pageId];
            Assert.IsType(page, typeof(T));
            return (T)_pages[pageId];
        } else {
            // cache miss - read from disk + add to buffer cache
            var buffer = _diskService.ReadPage(pageId);
            var page = Page.Create<T>(pageId, buffer);
            this._pages[pageId] = page;
            return page;
        }
    }

    public void Add(int pageId, PageBuffer buffer){
        _pages.Add(pageId, buffer);
    }

    public void Create(PageType pageType)
}