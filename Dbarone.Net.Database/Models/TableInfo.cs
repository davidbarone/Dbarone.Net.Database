/// <summary>
/// Table metadata.
/// </summary>
public class TableInfo {
    public string TableName { get; set; } = default!;
    public bool IsSystemTable { get; set; }
    public int FirstPageId { get; set; }
    public int LastPageId { get; set; }
    public int ColumnPageId { get; set; }
}