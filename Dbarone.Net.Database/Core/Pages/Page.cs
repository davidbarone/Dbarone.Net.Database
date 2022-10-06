namespace Dbarone.Net.Database;
using Dbarone.Net.Assertions;
using Dbarone.Net.Proxy;

/// <summary>
/// Base class for all pages.
/// </summary>
public class Page
{
    private PageBuffer _buffer;
    protected IPageHeader _headers;
    protected IList<IPageData> _data;

    protected virtual Type PageHeaderType { get { throw new NotImplementedException("Not implemented."); } }

    protected virtual Type PageDataType { get { throw new NotImplementedException("Not implemented."); } }
    protected IEnumerable<ColumnInfo> DataColumns { get; set; }

    /// <summary>
    /// Gets the column structure of each data row. By default returns the columns for the type `this.PageDataType`.
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerable<ColumnInfo> GetDefaultDataColumns()
    {
        if (this.PageDataType == typeof(DictionaryPageData))
        {
            throw new Exception($"Cannot get columns for data type: {this.PageDataType.Name}.");
        }
        return Serializer.GetColumnsForType(this.PageDataType);
    }

    /// <summary>
    /// Header information.
    /// </summary>
    public virtual IPageHeader Headers() { throw new NotImplementedException("Not implemented."); }

    /// <summary>
    /// Data within the page. The data is indexed using the slots array
    /// </summary>
    public virtual IEnumerable<IPageData> Data() { throw new NotImplementedException("Not implemented."); }

    /// <summary>
    /// The slot indexes.
    /// </summary>
    public IList<ushort> Slots { get; set; }

    private void Hydrate(PageBuffer buffer)
    {
        // Hydrate Headers
        var headerLength = buffer.ReadUInt16(0);    // first 2 bytes of buffer are total length.
        var headerBuf = buffer.Slice(0, headerLength);
        this._headers = (PageHeader)Serializer.Deserialize(this.PageHeaderType, headerBuf);

        // Hydrate slots
        var slotIndex = Global.PageSize - 2;
        for (int slot = 0; slot < this._headers.SlotsUsed; slot++)
        {
            var dataIndex = buffer.ReadUInt16(slotIndex);
            this.Slots.Add(dataIndex);

            // add data
            var totalLength = buffer.ReadUInt16(Global.PageHeaderSize + dataIndex); // First 2 bytes of each record store the record total length.
            var b = buffer.Slice(dataIndex + Global.PageHeaderSize, totalLength);

            if (this.PageDataType == typeof(DictionaryPageData))
            {
                // dictionary data
                var dict = (IDictionary<string, object>)Serializer.DeserializeDictionary(this.DataColumns, b);
                this._data.Add(new DictionaryPageData(dict!));
                slotIndex = slotIndex - 2;
            }
            else
            {
                // POCO data
                var item = (PageData)Serializer.Deserialize(this.PageDataType, b);
                this._data.Add(item);
                slotIndex = slotIndex - 2;
            }
        }
    }

    /// <summary>
    /// Instantiates a new Page object.
    /// </summary>
    /// <param name="pageId">The page id.</param>
    /// <param name="buffer"></param>
    public Page(int pageId, PageBuffer buffer, PageType pageType, IEnumerable<ColumnInfo>? dataColumns = null)
    {
        this._buffer = buffer;
        this._data = new List<IPageData>();
        this.Slots = new List<ushort>();
        this._headers = (IPageHeader)Activator.CreateInstance(this.PageHeaderType)!;
        if (dataColumns != null)
        {
            this.DataColumns = dataColumns;
        }
        else
        {
            this.DataColumns = GetDefaultDataColumns();
        }

        if (!buffer.IsEmpty())
        {
            Hydrate(buffer);
        }

        // Create proxy for header to set the IsDirty flag whenever any header property changes.
        CreateHeaderProxy();

        if (buffer.IsEmpty())
        {
            // If new page, set defaults. These will automatically set the IsDirty flag as interceptor created above.
            this.Headers().PageId = pageId;
            this.Headers().PageType = pageType;
            this.Headers().SlotsUsed = 0;
        }

        Assert.Equals(pageId, this.Headers().PageId);
    }

    /// <summary>
    /// Implement in a subclass to create header proxy.
    /// </summary>
    /// <param name="headers"></param>
    /// <returns></returns>
    public virtual void CreateHeaderProxy() { throw new NotImplementedException(); }

    /// <summary>
    /// Serialise row object. For page types that use
    /// DictionaryPageData (i.e. data pages), we get
    /// the inner dictionary.
    /// </summary>
    /// <param name="row">The row to serialise.</param>
    /// <returns>A byte[] array</returns>
    public byte[] SerializeDataRow(object row)
    {
        var dictionaryRow = row as DictionaryPageData;
        byte[]? buffer = null;
        if (dictionaryRow != null)
        {
            buffer = Serializer.SerializeDictionary(this.DataColumns, dictionaryRow.Row);
        }
        else
        {
            buffer = Serializer.Serialize(this.DataColumns, row);
        }
        return buffer;
    }

    /// <summary>
    /// Gets the data at a particular slot
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    public IPageData GetRowAtSlot(int slot){
        var slots = this.Headers().SlotsUsed;
        Assert.Between(slot, 0, slots - 1);
        return this._data[slot];
    }

    /// <summary>
    /// Adds data row to the page.
    /// </summary>
    /// <param name="row"></param>
    public void AddDataRow(object row)
    {
        Assert.IsType(row, this.PageDataType);

        var buffer = SerializeDataRow(row);

        if (!this.CanAddRowToPage(buffer.Length))
        {
            throw new Exception("Insufficient room on page.");
        }

        // Update page
        this.Headers().SlotsUsed++;
        this.Slots.Add(this.Headers().FreeOffset);
        this.Headers().FreeOffset += (ushort)buffer.Length;
        this._data.Add((row as IPageData)!);
    }

    public void AddDataRowFromBuffer(byte[] buffer)
    {
        if (!this.CanAddRowToPage(buffer.Length))
        {
            throw new Exception("Insufficient room on page.");
        }

        // Update page
        this.Headers().SlotsUsed++;
        this.Slots.Add(this.Headers().FreeOffset);
        this.Headers().FreeOffset += (ushort)buffer.Length;

        // Get object from buffer:
        if (this.PageDataType == typeof(DictionaryPageData))
        {
            // dictionary data
            var dict = (IDictionary<string, object>)Serializer.DeserializeDictionary(this.DataColumns, buffer);
            this._data.Add(new DictionaryPageData(dict!));
        }
        else
        {
            // POCO data
            var item = (PageData)Serializer.Deserialize(this.PageDataType, buffer);
            this._data.Add(item);
        }
    }

    /// <summary>
    /// Sets the IsDirty to true if any setter is called.
    /// </summary>
    /// <param name="interceptorArgs"></param>
    public static void IsDirtyInterceptor<TPageHeaderInterface>(InterceptorArgs<TPageHeaderInterface> interceptorArgs) where TPageHeaderInterface : IPageHeader
    {
        // Sets the IsDirty flag if any setter on header object is called.
        if (
            interceptorArgs.BoundaryType == BoundaryType.After &&
            interceptorArgs.Target != null &&
            interceptorArgs.TargetMethod.Name.Substring(0, 4) == "set_")
        {
            interceptorArgs.Target.IsDirty = true;
        }
    }

    /// <summary>
    /// Returns the current page as a PageBuffer. Used to write dirty pages back to disk
    /// </summary>
    /// <returns></returns>
    public PageBuffer ToPageBuffer()
    {
        byte[] b = new byte[8192];
        PageBuffer buffer = new PageBuffer(b, this._headers.PageId);

        // Headers - we need to serialise the original header, not the proxy
        var headerType = this.Headers().GetType();
        var pi = headerType.GetProperty("Target");
        var h = this.Headers();
        if (pi != null)
        {
            h = (IPageHeader)pi.GetValue(this.Headers())!;
        }
        var headerBytes = Serializer.Serialize(h);
        Assert.NotGreaterThan(headerBytes.Length, Global.PageHeaderSize);
        buffer.Write(headerBytes, 0);

        // Serialize slots
        var data = this.Data().ToList();
        var slotIndex = Global.PageSize - 2;
        for (int slot = 0; slot < this._headers.SlotsUsed; slot++)
        {
            var dataIndex = this.Slots[slot];
            buffer.Write(dataIndex, slotIndex);

            // Serialize data
            // totalLength is the second UInt from start.
            var dataBytes = SerializeDataRow(data[slot]);
            buffer.Write(dataBytes, dataIndex + Global.PageHeaderSize);

            slotIndex = slotIndex - 2;
        }
        return buffer;
    }

    /// <summary>
    /// Check whether row will fit on current page
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
    public bool CanAddRowToPage(int rowBuferSize)
    {
        return (Global.PageSize                                                             // Page size
            - Global.PageHeaderSize                                                         // Ignore page header
            - this.Headers().FreeOffset                                                     // Space already used by data rows
            - ((this.Headers().SlotsUsed + 1) * Types.GetByDataType(DataType.UInt16).Size)  // Slot table (including extra slot for new row)
            - rowBuferSize)                                                                 // Data to be written
        >= 0;
    }

    /// <summary>
    /// Returns the number of bytes available for the specified slot. Used when updating data rows in slots.
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    public int GetAvailableSpaceForSlot(short slot)
    {
        var slots = this.Headers().SlotsUsed;
        Assert.Between(slot, 0, slots - 1);
        if (slot == slots - 1)
        {
            // last slot - space available includes free padding at end of data
            return Global.PageSize                                                             // Page size
            - Global.PageHeaderSize                                                         // Ignore page header
            - this.Headers().FreeOffset                                                     // Space already used by data rows
            - ((this.Headers().SlotsUsed + 1) * Types.GetByDataType(DataType.UInt16).Size);  // Slot table (including extra slot for new row)
        }
        else
        {
            // not last slot. Return offset of next slot - current slot
            return this.Slots[slot + 1] - this.Slots[slot];
        }
    }

    /// <summary>
    /// Create a new page
    /// </summary>
    /// <param name="pageId"></param>
    /// <param name="buffer"></param>
    public static void Create(int pageId, PageBuffer buffer)
    {


    }
}