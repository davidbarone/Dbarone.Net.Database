namespace Dbarone.Net.Database;

/// <summary>
/// Base header information for all pages.
/// </summary>
public class PageHeader : IPageHeader
{
    public int PageId { get; set; }
    public int? PrevPageId { get; set; }
    public int? NextPageId { get; set; }
    
    /// <summary>
    /// If the page is related to a table or other parent object, this stores the id of that object.
    /// </summary>
    public int? ParentObjectId { get; set; }
    public int SlotsUsed { get; set; }
    public int TransactionId { get; set; }
    public ushort FreeOffset { get; set; } = 0!;
    public bool IsUnused { get; set; }
}

public interface IPageHeader
{
    int PageId { get; set; }
    int? PrevPageId { get; set; }
    int? NextPageId { get; set; }
    int? ParentObjectId { get; set; }
    int SlotsUsed { get; set; }
    int TransactionId { get; set; }

    /// <summary>
    /// Offset of the next free area on the data part of the page (excluding header)
    /// </summary>
    ushort FreeOffset { get; set; }
    bool IsUnused { get; set; }
}