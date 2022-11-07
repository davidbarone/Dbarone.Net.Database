namespace Dbarone.Net.Database;
using Dbarone.Net.Assertions;
using Dbarone.Net.Mapper;
using Dbarone.Net.Extensions.String;
using Dbarone.Net.Extensions.Object;

/// <summary>
/// In memory cache of pages as they are modified or read from disk. Dirty pages are written back to disk with a CHECKPOINT command.
/// </summary>
public class BufferManager : IBufferManager
{
    private TextEncoding textEncoding = TextEncoding.UTF8;
    public BufferManager(DiskService diskService)
    {
        this._diskService = diskService;
    }

    public void SaveDirtyPages()
    {
        foreach (var key in _pages.Keys)
        {
            var page = _pages[key];
            if (page.IsDirty)
            {
                var pageBuffer = SerialisePage(page)!;
                _diskService.WritePage(key, pageBuffer);
                page.IsDirty = false;
            }
        }
    }

    /// <summary>
    /// Marks a page as free
    /// </summary>
    /// <param name="page"></param>
    public void MarkFree(Page page)
    {
        page.MarkFree();
        page.Headers().PrevPageId = null;

        // Add to the start of the single-linked free page chain
        var boot = this.GetPage<BootPage>(0);
        boot.Headers().FirstFreePageId = page.Headers().PageId;
        int? nextId = boot.Headers().FirstFreePageId;
        if (nextId == null)
        {
            page.Headers().NextPageId = null;
        }
        else
        {
            var nextPage = GetPage(nextId.Value);
            nextPage.Headers().PrevPageId = page.Headers().PageId;
            page.Headers().NextPageId = nextId;
        }
    }

    private DiskService _diskService;
    private Dictionary<int, Page> _pages = new Dictionary<int, Page>();

    private PageType GetPageType(PageBuffer pageBuffer)
    {
        // Page type (starting at offset: 0)
        return (PageType)pageBuffer.ReadByte(0);
    }

    public string DebugPages()
    {
        var output = $"{"PageId".Justify(8, Justification.RIGHT)} {"PageType".Justify(16, Justification.RIGHT)} {"Prev".Justify(8, Justification.RIGHT)} {"Next".Justify(8, Justification.RIGHT)} {"Parent".Justify(8, Justification.RIGHT)} {"Slots".Justify(8, Justification.RIGHT)} {"Tran".Justify(8, Justification.RIGHT)} {"Dirty".Justify(8, Justification.RIGHT)} {"Free".Justify(8, Justification.RIGHT)}{Environment.NewLine}";
        output += $"{"------".Justify(8, Justification.RIGHT)} {"--------".Justify(16, Justification.RIGHT)} {"----".Justify(8, Justification.RIGHT)} {"----".Justify(8, Justification.RIGHT)} {"------".Justify(8, Justification.RIGHT)} {"-----".Justify(8, Justification.RIGHT)} {"----".Justify(8, Justification.RIGHT)} {"-----".Justify(8, Justification.RIGHT)} {"----".Justify(8, Justification.RIGHT)}{Environment.NewLine}";

        var pageCount = this._diskService.PageCount;
        for (int i = 0; i < pageCount; i++)
        {
            var page = GetPage(i);
            output += $"{page.Headers().PageId.ToString().Justify(8, Justification.RIGHT)} {page.PageType.ToString().Justify(16, Justification.RIGHT)} {page.Headers().PrevPageId.ToString().Justify(8, Justification.RIGHT)} {page.Headers().NextPageId.ToString().Justify(8, Justification.RIGHT)} {page.Headers().ParentObjectId.ToString().Justify(8, Justification.RIGHT)} {page.Headers().SlotsUsed.ToString().Justify(8, Justification.RIGHT)} {page.Headers().TransactionId.ToString().Justify(8, Justification.RIGHT)} {page.IsDirty.ToString().Justify(8, Justification.RIGHT)} {page.Headers().FreeOffset.ToString().Justify(8, Justification.RIGHT)}{Environment.NewLine}";
        }
        return output;
    }

    public string DebugPage(int pageId)
    {
        var page = GetPage(pageId);

        var output = @$"PageType: {page.PageType.ToString()}
IsDirty: {page.IsDirty}{Environment.NewLine}";

        var properties = page.Headers().GetType().GetProperties().Where(p => p.PropertyType.IsValueType || p.PropertyType == typeof(string));
        foreach (var property in properties)
        {
            output += $"Headers.{property.Name}: {property.GetValue(page.Headers())}{Environment.NewLine}";
        }
        for (int i = 0; i < page.Headers().SlotsUsed; i++)
        {
            output += $"{Environment.NewLine}";
            var status = page.Statuses[i];
            var statusString = $"[{(status.HasFlag(RowStatus.Deleted) ? 'D' : ' ')}{(status.HasFlag(RowStatus.Overflow) ? 'O' : ' ')}{(status.HasFlag(RowStatus.Null) ? 'N' : ' ')}]";
            output += $"Slot #{i}: Offset: {page.Slots[i]}, Status Flags: {statusString}, Type: {page._data[i].GetType().Name}{Environment.NewLine}";
            output += $"Slot #{i} Values:{Environment.NewLine}";
            var dict = page._data[i].ToDictionary();
            output += string.Join(Environment.NewLine, dict.Keys.Select(k => $" - {k}: {dict[k]}"));
            output += Environment.NewLine;
        }
        return output;
    }

    /// <summary>
    /// Gets a page.
    /// </summary>
    /// <param name="pageId"></param>
    /// <returns></returns>
    public Page GetPage(int pageId)
    {
        if (_pages.ContainsKey(pageId))
        {
            // cache hit
            var page = _pages[pageId];
            Assert.AssignableFrom(page, typeof(Page));
            return _pages[pageId];
        }
        else
        {
            // cache miss - read from disk + add to buffer cache
            var buffer = _diskService.ReadPage(pageId);
            var pageType = GetPageType(buffer);

            Page? page = null;
            if (pageType == PageType.Boot) page = new BootPage();
            else if (pageType == PageType.SystemTable) page = new SystemTablePage();
            else if (pageType == PageType.SystemColumn) page = new SystemColumnPage();
            else if (pageType == PageType.Data) page = new DataPage();
            else if (pageType == PageType.Overflow) page = new OverflowPage();
            else throw new Exception("Unable to create a new page.");

            // Hydrate the page from the buffer
            HydratePage(page, buffer);

            // Create header proxy
            page.CreateHeaderProxy();

            // Add to cache, and return
            this._pages[pageId] = page;

            // If Boot page, we set the text encoding for subsequent pages
            // Page0 is always read using default encoding of UTF-8.
            if (pageType == PageType.Boot && pageId == 0)
            {
                this.textEncoding = (page as BootPage)!.Headers().TextEncoding;
            }

            return page;
        }
    }

    /// <summary>
    /// Gets a page from the buffer cache. If page not present, reads from disk
    /// </summary>
    /// <param name="pageId"></param>
    /// <returns></returns>
    public T GetPage<T>(int pageId) where T : Page
    {
        return (T)GetPage(pageId);
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
        else if (typeof(T) == typeof(OverflowPage)) page = (T)(object)new OverflowPage(pageId);
        else throw new Exception("Unable to create a new page.");

        if (prevPageId != null)
        {
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
        if (linkedPage != null)
        {
            linkedPage.Headers().NextPageId = pageId;
        }

        // Initialise page
        return InitialisePage<T>(pageId, parentObjectId, (linkedPage != null) ? linkedPage.Headers().PageId : null);
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
    public byte[] SerialiseRow(object row, RowStatus rowStatus, IEnumerable<ColumnInfo> columns)
    {
        var dictionaryRow = row as DictionaryPageData;
        byte[]? buffer = null;
        if (dictionaryRow != null)
        {
            buffer = Serializer.SerializeDictionary(columns, dictionaryRow.Row, rowStatus, textEncoding);
        }
        else
        {
            buffer = Serializer.Serialize(columns, row, rowStatus, textEncoding);
        }
        return buffer;
    }

    private DeserializationResult<IPageData> DeserialiseRow(Page page, PageBuffer buffer, int dataIndex)
    {
        // add data
        if (page.PageDataType == typeof(BufferPageData))
        {
            // for overflow page, page stores single cell - length of buffer available from headers
            var totalLength = page.Headers().FreeOffset;
            var b = buffer.Slice(dataIndex + Global.PageHeaderSize, totalLength);
            return new DeserializationResult<IPageData>(new BufferPageData(b), RowStatus.None);
        }
        else
        {
            var totalLength = buffer.ReadUInt16(Global.PageHeaderSize + dataIndex); // First 2 bytes of each record store the record total length.
            var b = buffer.Slice(dataIndex + Global.PageHeaderSize, totalLength);
            if (page.PageDataType == typeof(DictionaryPageData))
            {
                if (Serializer.GetRowStatus(b).HasFlag(RowStatus.Overflow))
                {
                    // overflow data
                    var result = Serializer.Deserialize<OverflowPointer>(b, textEncoding);
                    return new DeserializationResult<IPageData>(result.Result, result.RowStatus);
                }
                else
                {
                    // dictionary data
                    var columnMeta = this.GetColumnsForPage(page);
                    var result = Serializer.DeserializeDictionary(columnMeta, b, textEncoding);
                    return new DeserializationResult<IPageData>(new DictionaryPageData(result.Result!), result.RowStatus);
                }
            }
            else
            {
                // POCO data
                var result = Serializer.Deserialize(page.PageDataType, b, textEncoding);
                return new DeserializationResult<IPageData>((IPageData)result.Result!, result.RowStatus);
            }
        }
    }

    /// <summary>
    /// Deserialises a buffer into an existing page.
    /// </summary>
    /// <param name="page">Page to deserialise into.</param>
    /// <param name="buffer">The buffer to deserialise.</param>
    private void HydratePage(Page page, PageBuffer buffer)
    {
        // Page type (starting at offset: 0)
        page.PageType = (PageType)buffer.ReadByte(0);

        // Hydrate Headers (starting at offset: 1)
        var headerLength = buffer.ReadUInt16(1);    // first 2 bytes of buffer are total length.
        var headerBuf = buffer.Slice(1, headerLength);
        page._headers = (PageHeader)Serializer.Deserialize(page.PageHeaderType, headerBuf, textEncoding).Result!;

        // Hydrate slots
        var slotIndex = Global.PageSize - 2;
        for (int slot = 0; slot < page._headers.SlotsUsed; slot++)
        {
            var dataIndex = buffer.ReadUInt16(slotIndex);
            page.Slots.Add(dataIndex);
            var deserialisationResult = DeserialiseRow(page, buffer, dataIndex);
            page._data.Add(deserialisationResult.Result!);
            page.Statuses.Add(deserialisationResult.RowStatus);
            slotIndex = slotIndex - 2;
        }
    }

    /// <summary>
    /// Converts a page to a PageBuffer which can be persisted to disk. Is called when persisting pages to disk.
    /// </summary>
    /// <param name="page">The page to convert.</param>
    /// <returns>Returns a PageBuffer representation of the page.</returns>
    private PageBuffer? SerialisePage(Page page)
    {
        byte[] b = new byte[8192];
        PageBuffer buffer = new PageBuffer(b, page.Headers().PageId);

        // Page Type (offset 0)
        buffer.Write(page.PageType, 0);

        // Headers - we need to serialise the original header, not the proxy
        // Offset 1
        var headerType = page.Headers().GetType();
        var pi = headerType.GetProperty("Target");
        var h = page.Headers();
        if (pi != null)
        {
            h = (IPageHeader)pi.GetValue(page.Headers())!;
        }
        var headerBytes = Serializer.Serialize(h, RowStatus.None, textEncoding);
        // Header must be less than 50 (PageType occupies 1 byte at start)
        Assert.LessThan(headerBytes.Length, Global.PageHeaderSize);
        buffer.Write(headerBytes, 1);

        // Serialize slots
        var data = page._data.ToList();
        var slotIndex = Global.PageSize - 2;
        for (ushort slot = 0; slot < page.Headers().SlotsUsed; slot++)
        {
            var dataIndex = page.Slots[slot];
            buffer.Write(dataIndex, slotIndex);

            // Serialize data
            // totalLength is the second UInt from start.
            var bufferPageData = data[slot] as BufferPageData;
            var overflowPointer = data[slot] as OverflowPointer;
            if (bufferPageData != null)
            {
                // For buffer page data (used in overflow pages), data already byte[].
                // Just write out.
                buffer.Write(bufferPageData.Buffer, dataIndex + Global.PageHeaderSize);
            }
            else if (overflowPointer != null)
            {
                // cell is an overflow pointer
                var columns = Serializer.GetColumnsForType(typeof(OverflowPointer));
                var dataBytes = SerialiseRow(data[slot], page.Statuses[slot] | RowStatus.Overflow, columns);
                buffer.Write(dataBytes, dataIndex + Global.PageHeaderSize);
            }
            else
            {
                // Normal data - must serialise.
                var columns = GetColumnsForPage(page);
                var dataBytes = SerialiseRow(data[slot], page.Statuses[slot], columns);
                buffer.Write(dataBytes, dataIndex + Global.PageHeaderSize);
            }
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
            if (page.PageType == PageType.Data)
            {
                if (page.GetType() == typeof(DataPage))
                {
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