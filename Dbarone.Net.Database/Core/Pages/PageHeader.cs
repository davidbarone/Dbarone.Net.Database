namespace Dbarone.Net.Database;

/// <summary>
/// Base header information for all pages.
/// </summary>
public class PageHeader : IPageHeader
{
    public PageType PageType { get; set; }
    public uint PageId { get; set; }
    public uint PrevPageId { get; set; }
    public uint NextPageId { get; set; }
    public uint SlotsUsed { get; set; }
    public uint TransactionId { get; set; }
    public bool IsDirty { get; set; }
}

public interface IPageHeader
{
    PageType PageType { get; set; }
    uint PageId { get; set; }
    uint PrevPageId { get; set; }
    uint NextPageId { get; set; }
    uint SlotsUsed { get; set; }
    uint TransactionId { get; set; }
    bool IsDirty { get; set; }
}