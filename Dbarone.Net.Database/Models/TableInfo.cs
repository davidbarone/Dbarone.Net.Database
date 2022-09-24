/// <summary>
/// Table metadata.
/// </summary>
public class TableInfo {
    public string TableName { get; set; } = default!;
    public bool IsSystemTable { get; set; }
    public int PageId { get; set; }
    public int ColumnPageId { get; set; }
}