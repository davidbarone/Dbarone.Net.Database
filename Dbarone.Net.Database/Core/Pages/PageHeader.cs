namespace Dbarone.Net.Database;

/// <summary>
/// Base header information for all pages.
/// </summary>
public class PageHeader : IPageHeader
{
    public PageType PageType { get; set; }
    public int PageId { get; set; }
    public int PrevPageId { get; set; }
    public int NextPageId { get; set; }
    public int SlotsUsed { get; set; }
    public int TransactionId { get; set; }
    public bool IsDirty { get; set; }
}

public interface IPageHeader
{
    PageType PageType { get; set; }
    int PageId { get; set; }
    int PrevPageId { get; set; }
    int NextPageId { get; set; }
    int SlotsUsed { get; set; }
    int TransactionId { get; set; }
    bool IsDirty { get; set; }
}