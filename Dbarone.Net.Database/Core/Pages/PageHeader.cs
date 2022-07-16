/// <summary>
/// Base header information for all pages.
/// </summary>
public class PageHeader {
    public PageType PageType { get; set; }
    public int PageId { get; set; }
    public int PrevPageId { get; set; }
    public int NextPageId { get; set; }
    public int SlotsUsed { get; set; }
    public int TransactionId { get; set; }
    public bool IsDirty { get; set; }
}