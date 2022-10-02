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
    /// Root data page. Can be either leaf data page, or tree node
    /// </summary>
    public int RootPageId { get; set; }

    /// <summary>
    /// Column metadata page id.
    /// </summary>
    public int ColumnPageId { get; set; }
}