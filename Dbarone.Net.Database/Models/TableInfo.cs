/// <summary>
/// Table metadata.
/// </summary>
public class TableInfo {
    public int ObjectId { get; set; }
    public string TableName { get; set; } = default!;
    public bool IsSystemTable { get; set; }
    public int FirstDataPageId { get; set; }
    public int LastDataPageId { get; set; }
    public int FirstColumnPageId { get; set; }
    public int LastColumnPageId { get; set; }
}