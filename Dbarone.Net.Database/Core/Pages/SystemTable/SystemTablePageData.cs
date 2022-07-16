namespace Dbarone.Net.Database;

/// <summary>
/// Item data for the SystemTablePage - each item contains table metadata.
/// </summary>
public class SystemTablePageData : PageData {
    public string TableName { get; set; } = default!;
    public bool IsSystemTable { get; set; }
    public int PageId { get; set; }
}