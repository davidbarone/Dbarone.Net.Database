namespace Dbarone.Net.Database;
using Dbarone.Net.Assertions;
using Dbarone.Net.Mapper;

/// <summary>
/// In memory cache of pages as they are modified or read from disk. Dirty pages are written back to disk with a CHECKPOINT command.
/// </summary>
public class BufferManager : IBufferManager
{
    public BufferManager(DiskService diskService)
    {
        this._diskService = diskService;
    }

    public void SaveDirtyPages()
    {
        foreach (var key in _pages.Keys)
        {
            var page = _pages[key];
            if (page.Headers().IsDirty)
            {
                var pageBuffer = SerialisePage(page)!;
                _diskService.WritePage(key, pageBuffer);
                page.Headers().IsDirty = false;
            }
        }
    }

    private DiskService _diskService;
    private Dictionary<int, Page> _pages = new Dictionary<int, Page>();

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
            if (typeof(T) == typeof(BootPage)) page = (T)(object)new BootPage();
            else if (typeof(T) == typeof(SystemTablePage)) page = (T)(object)new SystemTablePage();
            else if (typeof(T) == typeof(SystemColumnPage)) page = (T)(object)new SystemColumnPage();
            else if (typeof(T) == typeof(DataPage)) page = (T)(object)new DataPage();
            else throw new Exception("Unable to create a new page.");

            // Hydrate the page from the buffer
            HydratePage(page, buffer);

            // Create header proxy
            page.CreateHeaderProxy();

            // Add to cache, and return
            this._pages[pageId] = page;
            return (T)page;
        }
    }

    /// <summary>
    /// Instantiates / creates a new page from thin air.
    /// </summary>
    /// <typeparam name="T">The page type.</typeparam>
    /// <param name="pageId">The page Id.</param>
    /// <param name="parentObjectId">Optional parent id. Required for some page types.</param>
    /// <returns>Returns a new instantiated page.</returns>
    /// <exception cref="Exception"></exception>
    private T InitialisePage<T>(int pageId, int? parentObjectId = null, int? prevPageId = null) where T : Page
    {
        //var buffer = _diskService.ReadPage(pageId);

        T? page = null;
        // Create page object - buffer not required.
        if (typeof(T) == typeof(BootPage)) page = (T)(object)new BootPage(pageId);
        else if (typeof(T) == typeof(SystemTablePage)) page = (T)(object)new SystemTablePage(pageId);
        else if (typeof(T) == typeof(SystemColumnPage)) page = (T)(object)new SystemColumnPage(pageId);
        else if (typeof(T) == typeof(DataPage)) page = (T)(object)new DataPage(pageId, parentObjectId);
        else throw new Exception("Unable to create a new page.");

        if (prevPageId!=null) {
            page.Headers().PrevPageId = prevPageId;
        }

        // Create header proxy
        page.CreateHeaderProxy();

        // For brand new pages, immediately set dirty flag
        page.SetDirty();

        // Add to cache, and return
        this._pages[pageId] = page;
        return (T)page;
    }

    public T CreatePage<T>(int? parentObjectId = null, Page? linkedPage = null) where T : Page
    {
        var pageId = _diskService.CreatePage();

        // Updated linked page
        if (linkedPage!=null){
            linkedPage.Headers().NextPageId = pageId;
        }

        // Initialise page
        return InitialisePage<T>(pageId, parentObjectId, (linkedPage!=null)?linkedPage.Headers().PageId:null);
    }

    #region Serialisation

    /// <summary>
    /// Serialise row object. For page types that use
    /// DictionaryPageData (i.e. data pages), we get
    /// the inner dictionary.
    /// </summary>
    /// <param name="row">The row to serialise.</param>
    /// <param name="columns">The column metadata.</param>
    /// <returns>A byte[] array</returns>
    public byte[] SerialiseRow(object row, IEnumerable<ColumnInfo> columns)
    {
        var dictionaryRow = row as DictionaryPageData;
        byte[]? buffer = null;
        if (dictionaryRow != null)
        {
            buffer = Serializer.SerializeDictionary(columns, dictionaryRow.Row);
        }
        else
        {
            buffer = Serializer.Serialize(columns, row);
        }
        return buffer;
    }

    /// <summary>
    /// Deserialises a buffer into an existing page.
    /// </summary>
    /// <param name="page">Page to deserialise into.</param>
    /// <param name="buffer">The buffer to deserialise.</param>
    private void HydratePage(Page page, PageBuffer buffer)
    {
        // Hydrate Headers
        var headerLength = buffer.ReadUInt16(0);    // first 2 bytes of buffer are total length.
        var headerBuf = buffer.Slice(0, headerLength);
        page._headers = (PageHeader)Serializer.Deserialize(page.PageHeaderType, headerBuf);

        // Hydrate slots
        var slotIndex = Global.PageSize - 2;
        for (int slot = 0; slot < page._headers.SlotsUsed; slot++)
        {
            var dataIndex = buffer.ReadUInt16(slotIndex);
            page.Slots.Add(dataIndex);

            // add data
            var totalLength = buffer.ReadUInt16(Global.PageHeaderSize + dataIndex); // First 2 bytes of each record store the record total length.
            var b = buffer.Slice(dataIndex + Global.PageHeaderSize, totalLength);

            if (page.PageDataType == typeof(DictionaryPageData))
            {
                // dictionary data
                var columnMeta = this.GetColumnsForPage(page);
                var dict = (IDictionary<string, object>)Serializer.DeserializeDictionary(columnMeta, b);
                page._data.Add(new DictionaryPageData(dict!));
                slotIndex = slotIndex - 2;
            }
            else
            {
                // POCO data
                var item = (PageData)Serializer.Deserialize(page.PageDataType, b);
                page._data.Add(item);
                slotIndex = slotIndex - 2;
            }
        }
    }

    /// <summary>
    /// Converts a page to a PageBuffer which can be persisted to disk.
    /// </summary>
    /// <param name="page">The page to convert.</param>
    /// <returns>Returns a PageBuffer representation of the page.</returns>
    private PageBuffer? SerialisePage(Page page)
    {
        byte[] b = new byte[8192];
        PageBuffer buffer = new PageBuffer(b, page.Headers().PageId);

        // Headers - we need to serialise the original header, not the proxy
        var headerType = page.Headers().GetType();
        var pi = headerType.GetProperty("Target");
        var h = page.Headers();
        if (pi != null)
        {
            h = (IPageHeader)pi.GetValue(page.Headers())!;
        }
        var headerBytes = Serializer.Serialize(h);
        Assert.NotGreaterThan(headerBytes.Length, Global.PageHeaderSize);
        buffer.Write(headerBytes, 0);

        // Serialize slots
        var data = page.Data().ToList();
        var slotIndex = Global.PageSize - 2;
        for (int slot = 0; slot < page.Headers().SlotsUsed; slot++)
        {
            var dataIndex = page.Slots[slot];
            buffer.Write(dataIndex, slotIndex);

            // Serialize data
            // totalLength is the second UInt from start.
            var columns = GetColumnsForPage(page);
            var dataBytes = SerialiseRow(data[slot], columns);
            buffer.Write(dataBytes, dataIndex + Global.PageHeaderSize);

            slotIndex = slotIndex - 2;
        }
        return buffer;
    }

    #endregion

    #region Metadata

    /// <summary>
    /// Gets the column information for a page.
    /// </summary>
    /// <param name="page">The page</param>
    /// <returns>Returns either static or configured columns.</returns>
    /// <exception cref="Exception"></exception>
    public IEnumerable<ColumnInfo> GetColumnsForPage(Page page)
    {
        if (page.PageDataType != typeof(DictionaryPageData))
        {
            // pages that have static data row types
            return Serializer.GetColumnsForType(page.PageDataType);
        }
        else
        {
            // The columns are configured at the parent object.
            if (page.Headers().PageType == PageType.Data)
            {
                if (page.GetType()==typeof(DataPage)) {
                    var parentObjectId = page.Headers().ParentObjectId;
                    var heap = new HeapTableManager<SystemColumnPageData, SystemColumnPage>(this, parentObjectId);
                    var mapper = Mapper.ObjectMapper<SystemColumnPageData, ColumnInfo>.Create();
                    return mapper.MapMany(heap.Scan());
                }
                throw new Exception("Cannot get columns for page.");
            }
            throw new Exception("Cannot get columns for page.");
        }
    }

    #endregion
}