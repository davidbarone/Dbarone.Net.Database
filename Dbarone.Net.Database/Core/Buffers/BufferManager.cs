namespace Dbarone.Net.Database;
using Dbarone.Net.Assertions;
using Dbarone.Net.Mapper;
using Dbarone.Net.Extensions.String;
using Dbarone.Net.Extensions.Object;

/// <summary>
/// In memory cache of pages as they are modified or read from disk. Dirty pages are written back to disk with a CHECKPOINT command.
/// </summary>
public abstract class BufferManager : IBufferManager, IStorage
{
    protected int PageSize { get; set; }
    protected Dictionary<int, Page> Cache = new Dictionary<int, Page>();

    public ISerializer Serializer { get; set; }

    public int Count
    {
        get { return Cache.Count(); }
    }

    /// <summary>
    /// Creates a new buffer manager with a given page size.
    /// </summary>
    /// <param name="pageSize"></param>
    public BufferManager(int pageSize, ISerializer serializer)
    {
        this.PageSize = pageSize;
        this.Serializer = serializer;
    }

    #region IStorage

    public abstract PageBuffer StorageRead(int pageId);

    public abstract void StorageWrite(PageBuffer page);

    public abstract int StoragePageCount();

    #endregion

    #region IBufferManager

    public int MaxPageId
    {
        get
        {
            var maxBufferId = this.Cache.Values.Max(c => c.PageId);
            var maxStorageId = StoragePageCount();
            return maxBufferId > maxStorageId ? maxBufferId : maxStorageId;
        }
    }

    public void DropCleanBuffers()
    {

    }

    public void Save()
    {
        foreach (var key in Cache.Keys)
        {
            var page = Cache[key];
            if (page.IsDirty)
            {
                var pageBuffer = Serializer.Serialize(page);
                this.StorageWrite(pageBuffer);
                page.IsDirty = false;
            }
        }
    }

    /// <summary>
    /// Create a page and return the new page id.
    /// </summary>
    /// <param name="pageType">The page type to create.</param>
    /// <returns>The page id created.</returns>
    public Page Create(PageType pageType)
    {
        // Get max pageid in buffer
        var nextBufferPageId = this.Cache.Values.Max(p => p.PageId) + 1;
        var nextStoragePageId = StoragePageCount();
        var nextPageId = nextBufferPageId > nextStoragePageId ? nextBufferPageId : nextStoragePageId;

        byte[] buffer = new byte[this.PageSize];
        var pb = new PageBuffer(buffer);
        pb.PageId = nextPageId;
        pb.PageType = pageType;
        var page = Serializer.Deserialize(pb);
        page.IsDirty = true;
        this.Cache[pb.PageId] = page;
        return page;
    }

    /// <summary>
    /// Marks a page as free
    /// </summary>
    /// <param name="page"></param>
    public void Clear(int pageId)
    {
        var page = this.Get(pageId);
        page.Clear();
    }

    /// <summary>
    /// Gets a page.
    /// </summary>
    /// <remarks>
    /// This method returns a page from the cache if present. Otherwise, it will read the page from storage.
    /// </remarks>
    /// <param name="pageId">The page id.</param>
    /// <returns>Returns a page.</returns>
    public Page Get(int pageId)
    {
        if (Cache.ContainsKey(pageId))
        {
            // cache hit
            var page = Cache[pageId];
            return Cache[pageId];
        }
        else
        {
            // cache miss - read from disk + add to buffer cache
            var buffer = StorageRead(pageId);
            var page = Serializer.Deserialize(buffer);
            this.Cache[pageId] = page;
            return page;
        }
    }

    /// <summary>
    /// Gets the database configuration from the boot page (page0).
    /// </summary>
    public BootData GetBootData()
    {
        var data = (BootData)this.Get(0).Header;
        return data;
    }

    #endregion
}
