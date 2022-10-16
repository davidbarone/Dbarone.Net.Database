namespace Dbarone.Net.Database;

/// <summary>
/// Item data for the SystemTablePage - each item contains table metadata.
/// </summary>
public class SystemTablePageData : PageData {
    
    /// <summary>
    /// Unique Id for the table.
    /// </summary>
    public int ObjectId { get; set; }
    
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
    public int FirstPageId { get; set; }

    /// <summary>
    /// Last data page.
    /// </summary>
    public int LastPageId { get; set; }

    /// <summary>
    /// Column metadata page id.
    /// </summary>
    public int ColumnPageId { get; set; }
}