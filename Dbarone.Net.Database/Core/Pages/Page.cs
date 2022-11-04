namespace Dbarone.Net.Database;
using Dbarone.Net.Assertions;
using Dbarone.Net.Proxy;

/// <summary>
/// Base class for all pages.
/// </summary>
public class Page
{
    public PageType PageType { get; set; }
    public bool IsDirty { get; set; }
    public IPageHeader _headers;
    
    /// <summary>
    /// Data row physically on page. Can be PageDataType, or OverflowPointer. 
    /// </summary>
    public IList<IPageData> _data;
    public virtual Type PageHeaderType { get { throw new NotImplementedException("Not implemented."); } }
    public virtual Type PageDataType { get { throw new NotImplementedException("Not implemented."); } }
    public IList<ushort> Slots { get; set; }
    public IList<RowStatus> Statuses { get; set; }
    
    /// <summary>
    /// Creates a new page.
    /// </summary>
    /// <param name="pageId">The page id.</param>
    /// <param name="pageType">The page type.</param>
    public Page(int pageId, int? parentObjectId, PageType pageType)
    {
        this._data = new List<IPageData>();
        this.Slots = new List<ushort>();
        this.Statuses = new List<RowStatus>();

        this._headers = (IPageHeader)Activator.CreateInstance(this.PageHeaderType)!;
        //this.DataColumns = GetDefaultDataColumns();
        this.PageType = pageType;
        this.Headers().PageId = pageId;
        this.Headers().ParentObjectId = parentObjectId;
        Assert.Equals(pageId, this.Headers().PageId);
    }

    /// <summary>
    /// Default constructor.
    /// </summary>
    public Page() {
        this._data = new List<IPageData>();
        this.Slots = new List<ushort>();
        this.Statuses = new List<RowStatus>();
        this._headers = (IPageHeader)Activator.CreateInstance(this.PageHeaderType)!;
     }

    /// <summary>
    /// Returns the header structure. Subclasses can provide the specific type.
    /// </summary>
    public virtual IPageHeader Headers() { throw new NotImplementedException("Not implemented."); }

    /// <summary>
    /// Must be implemented in a subclass to create the header proxy.
    /// </summary>
    /// <param name="headers"></param>
    /// <returns></returns>
    public virtual void CreateHeaderProxy() { throw new NotImplementedException(); }

    /// <summary>
    /// Manually sets the page to dirty.
    /// </summary>
    public void SetDirty()
    {
        this.IsDirty = true;
    }

    #region Row Methods

    /// <summary>
    /// Gets the data row at a particular slot index.
    /// </summary>
    /// <param name="slot">The slot index.</param>
    /// <returns>Returns the row at the specified index.</returns>
    public IPageData GetRowAtSlot(int slot)
    {
        var slots = this.Headers().SlotsUsed;
        Assert.Between(slot, 0, slots - 1);
        return this._data[slot];
    }

    public RowStatus GetRowStatus(ushort slot) {
        return this.Statuses[slot];
    }

    public void SetRowStatus(ushort slot, RowStatus status) {
        this.Statuses[slot] = this.Statuses[slot] | status;
        this.IsDirty = true;
    }

    public void ClearRowStatus(ushort slot, RowStatus status) {
        this.Statuses[slot] = this.Statuses[slot] & (~status);
        this.IsDirty = true;
    }

    /// <summary>
    /// Adds a data row to a page. Both the row and the buffer are required, so that the
    /// free space can be checked prior to adding.
    /// </summary>
    /// <param name="row">The row to be added.</param>
    /// <param name="buffer">The buffer representation of the row.</param>
    /// <returns>The slot id of the new row.</returns>
    /// <exception cref="Exception">Throws exception if insufficient room on page for row.</exception>
    public int AddDataRow(object row, byte[] buffer)
    {
        Assert.IsType(row, this.PageDataType);
        
        if (buffer.Length>GetFreeRowSpace()){
            throw new Exception("Insufficient room on page.");
        }

        // Update page
        this.Headers().SlotsUsed++;
        this.Slots.Add(this.Headers().FreeOffset);
        this.Statuses.Add(RowStatus.None);
        this.Headers().FreeOffset += (ushort)buffer.Length;
        this._data.Add((row as IPageData)!);
        return this.Headers().SlotsUsed;
    }

    /// <summary>
    /// Similar to AddDataRow(), but used to add the overflow pointer record in the main data page.
    /// </summary>
    /// <param name="row"></param>
    /// <param name="buffer"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public int AddOverflowPointerDataRow(object row, byte[] buffer) {
        Assert.IsType(row, typeof(OverflowPointer));
        
        if (buffer.Length>GetFreeRowSpace()){
            throw new Exception("Insufficient room on page.");
        }

        // Update page
        this.Headers().SlotsUsed++;
        this.Slots.Add(this.Headers().FreeOffset);
        this.Statuses.Add(RowStatus.Overflow);
        this.Headers().FreeOffset += (ushort)buffer.Length;
        this._data.Add((row as IPageData)!);
        return this.Headers().SlotsUsed;
    }

    /// <summary>
    /// Updates a data row in place. Can only update the row if the updated
    /// value serialised fits into the existing data row space. If not, an
    /// exception is thrown.
    /// </summary>
    /// <param name="slot">The slot to update the data row of.</param>
    /// <param name="row">The updated value.</param>
    /// <param name="buffer">The buffer representation of the updated value.</param>
    /// <exception cref="Exception">Throws an exception if the updated value does not fit into the existing slot space.</exception>
    public void UpdateDataRow(ushort slot, object row, byte[] buffer){
        Assert.IsType(row, this.PageDataType);
        Assert.Between(slot, 0, this.Headers().SlotsUsed - 1);

        if (buffer.Length>GetAvailableSpaceForSlot(slot)){
            throw new Exception($"Insufficient room in slot {slot}.");
        }

        // Update page
        if (slot<this.Headers().SlotsUsed-1) {
            // not the last slot - nothing required - updated into existing slot that fits
            // There may be some unused space though - will need to add compaction function later.
        } else {
            // Get previous slot, and add updated size
            if (this.Slots.Count()==1){
                this.Headers().FreeOffset = (ushort)buffer.Length;
            } else
            {
                this.Headers().FreeOffset = (ushort)(this.Slots[slot - 1] + buffer.Length);
            }
        }

        this._data[slot] = (row as IPageData)!;

        // Set page dirty
        this.SetDirty();
    }

    /// <summary>
    /// Returns the number of bytes available for the specified slot. Used when updating data rows in slots.
    /// </summary>
    /// <param name="slot">Slot index.</param>
    /// <returns></returns>
    public int GetAvailableSpaceForSlot(ushort slot)
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
    /// Row is considered as requiring overflow storage if > 1/4 free space on new page.
    /// </summary>
    /// <returns></returns>
    public int GetOverflowThresholdSize() {
        return (Global.PageSize - Global.PageHeaderSize - (4 * Types.GetByDataType(DataType.UInt16).Size)) / 4;
    }
    #endregion

    #region Proxy

    /// <summary>
    /// Sets the IsDirty to true if any setter is called.
    /// </summary>
    /// <param name="interceptorArgs"></param>
    public void IsDirtyInterceptor<TPageHeaderInterface>(InterceptorArgs<TPageHeaderInterface> interceptorArgs) where TPageHeaderInterface : IPageHeader
    {
        // Sets the IsDirty flag if any setter on header object is called.
        if (
            interceptorArgs.BoundaryType == BoundaryType.After &&
            interceptorArgs.Target != null &&
            interceptorArgs.TargetMethod.Name.Substring(0, 4) == "set_")
        {
            this.IsDirty = true;
        }
    }

    #endregion
}