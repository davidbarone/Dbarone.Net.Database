namespace Dbarone.Net.Database;
using Dbarone.Net.Assertions;
using Dbarone.Net.Proxy;

/// <summary>
/// Base class for all pages.
/// </summary>
public class Page
{
    public IPageHeader _headers;
    public IList<IPageData> _data;
    public IEnumerable<ColumnInfo> DataColumns { get; set; }
    public virtual Type PageHeaderType { get { throw new NotImplementedException("Not implemented."); } }
    public virtual Type PageDataType { get { throw new NotImplementedException("Not implemented."); } }
    public IList<ushort> Slots { get; set; }

    /// <summary>
    /// Creates a new page.
    /// </summary>
    /// <param name="pageId">The page id.</param>
    /// <param name="pageType">The page type.</param>
    public Page(int pageId, int? parentObjectId, PageType pageType)
    {
        this._data = new List<IPageData>();
        this.Slots = new List<ushort>();
        this._headers = (IPageHeader)Activator.CreateInstance(this.PageHeaderType)!;
        //this.DataColumns = GetDefaultDataColumns();
        this.Headers().PageId = pageId;
        this.Headers().PageType = pageType;
        this.Headers().ParentObjectId = parentObjectId;
        Assert.Equals(pageId, this.Headers().PageId);
    }

    /// <summary>
    /// Default constructor.
    /// </summary>
    public Page() { }

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
    /// Implement in a subclass to create header proxy.
    /// </summary>
    /// <param name="headers"></param>
    /// <returns></returns>
    public virtual void CreateHeaderProxy() { throw new NotImplementedException(); }

    /// <summary>
    /// Manually set the page to dirty.
    /// </summary>
    public void SetDirty()
    {
        this.Headers().IsDirty = true;
    }


    /// <summary>
    /// Gets the data at a particular slot
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    public IPageData GetRowAtSlot(int slot)
    {
        var slots = this.Headers().SlotsUsed;
        Assert.Between(slot, 0, slots - 1);
        return this._data[slot];
    }

    /// <summary>
    /// Adds a data row to a page. Both the row and the buffer are required, so that the
    /// free space can be checked prior to adding.
    /// </summary>
    /// <param name="row">The row to be added.</param>
    /// <param name="buffer">The buffer representation of the row.</param>
    /// <exception cref="Exception"></exception>
    public void AddDataRow(object row, byte[] buffer)
    {
        Assert.IsType(row, this.PageDataType);

        if (buffer.Length>GetFreeRowSpace()){
            throw new Exception("Insufficient room on page.");
        }

        // Update page
        this.Headers().SlotsUsed++;
        this.Slots.Add(this.Headers().FreeOffset);
        this.Headers().FreeOffset += (ushort)buffer.Length;
        this._data.Add((row as IPageData)!);
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
    /// Returns the space free on the page that can be used to store new row data.
    /// </summary>
    /// <returns></returns>
    public int GetFreeRowSpace()
    {
        return Global.PageSize                                                             // Page size
            - Global.PageHeaderSize                                                         // Ignore page header
            - this.Headers().FreeOffset                                                     // Space already used by data rows
            - ((this.Headers().SlotsUsed + 1) * Types.GetByDataType(DataType.UInt16).Size); // Slot table (including extra slot for new row)
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
    /// Create a new page that is not yet persisted to disk.
    /// </summary>
    /// <param name="pageId"></param>
    /// <param name="buffer"></param>
    public static void Create<TPageType>(int pageId, PageBuffer buffer)
    {


    }
}