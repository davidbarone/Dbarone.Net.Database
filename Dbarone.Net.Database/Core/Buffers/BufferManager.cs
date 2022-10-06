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

    public void SavePages(){
        foreach (var key in _pages.Keys) {
            var page = _pages[key];
            if (page.Headers().IsDirty) {
                var pageBuffer = page.ToPageBuffer();
                _diskService.WritePage(key, pageBuffer);
                page.Headers().IsDirty = false;
            }
        }
    }

    private DiskService _diskService;
    private Dictionary<int, Page> _pages = new Dictionary<int, Page>();

    /// <summary>
    /// Gets the column information for a data page, as separate page
    /// reads are required for this information. Other page types can
    /// get the column information automatically.
    /// </summary>
    /// <param name="pageId">The page id.</param>
    /// <returns>The column information.</returns>
    private IEnumerable<ColumnInfo> GetDataColumnsForDataPage(int pageId){
        // Get the column structure from the parent -> columns
        var systemTablePage = this.GetPage<SystemTablePage>(1);
        var table = systemTablePage.Data().First(d => d.RootPageId == pageId);
        var systemColumnPage = this.GetPage<SystemColumnPage>(table.ColumnPageId);
        var mapper = Mapper.ObjectMapper<SystemColumnPageData, ColumnInfo>.Create();
        return mapper.MapMany(systemColumnPage.Data());
    }

    /// <summary>
    /// Gets a page from the buffer cache. If page not present, reads from disk
    /// </summary>
    /// <param name="pageId"></param>
    /// <returns></returns>
    public T GetPage<T>(int pageId) where T : Page
    {
        if (_pages.ContainsKey(pageId))
        {
            // cache hit
            var page = _pages[pageId];
            Assert.AssignableFrom(page, typeof(Page));
            return (T)_pages[pageId];
        }
        else
        {
            // cache miss - read from disk + add to buffer cache
            var buffer = _diskService.ReadPage(pageId);
            T? page = null;
            if (typeof(T) == typeof(BootPage)) page = (T)(object)new BootPage(pageId, buffer);
            else if (typeof(T) == typeof(SystemTablePage)) page = (T)(object)new SystemTablePage(pageId, buffer);
            else if (typeof(T) == typeof(SystemColumnPage)) page = (T)(object)new SystemColumnPage(pageId, buffer);
            else if (typeof(T) == typeof(DataPage)) page = (T)(object)new DataPage(pageId, buffer, this.GetDataColumnsForDataPage(pageId));
            else throw new Exception("Unable to create a new page.");
            this._pages[pageId] = page;
            return (T)page;
        }
    }

    public int CreatePage(PageType pageType)
    {
        var pageId = _diskService.CreatePage(pageType);
        return pageId;
    }
}