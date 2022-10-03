namespace Dbarone.Net.Database;

/// <summary>
/// Data row in a tree node page. Contains information about a child page.
/// </summary>
public class TreeNodePageData : PageData {
    
    /// <summary>
    /// The child page id.
    /// </summary>
    public string ChildPageId { get; set; } = default!;
    
    /// <summary>
    /// Number of rows in the page (including deleted rows).
    /// </summary>
    public int Rows { get; set; }

    /// <summary>
    /// Number of deleted rows in the page.
    /// </summary>
    public int DeletedRows { get; set; }

    /// <summary>
    /// The free space (in bytes) on the data page.
    /// </summary>
    public int FreeSpace { get; set; }
}