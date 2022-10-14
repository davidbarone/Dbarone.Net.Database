/// <summary>
/// Represents a physical location of a data row.
/// </summary>
public class DataRowLocation {
    /// <summary>
    /// Page id of the data row (zero based).
    /// </summary>
    public int PageId { get; set; }
    
    /// <summary>
    /// Slot index of the data row (zero based).
    /// </summary>
    public ushort Slot { get; set; }

    public DataRowLocation(int pageId, ushort slot) {
        this.PageId = pageId;
        this.Slot = slot;
    }
}