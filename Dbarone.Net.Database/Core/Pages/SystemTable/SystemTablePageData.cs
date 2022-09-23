namespace Dbarone.Net.Database;

/// <summary>
/// Item data for the SystemTablePage - each item contains table metadata.
/// </summary>
public class SystemTablePageData : PageData {
    
    /// <summary>
    /// The table name.
    /// </summary>
    public string TableName { get; set; } = default!;
    
    /// <summary>
    /// Set to true if a system table.
    /// </summary>
    public bool IsSystemTable { get; set; }
    
    /// <summary>
    /// First data page.
    /// </summary>
    public int PageId { get; set; }
    
    /// <summary>
    /// Column metadata page id.
    /// </summary>
    public int ColumnPageId { get; set; }
}