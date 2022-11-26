/// <summary>
/// Index metadata.
/// </summary>
public class IndexInfo
{
    public int ObjectId { get; set; }
    public string IndexName { get; set; } = default!;
    public int ParentObjectId { get; set; }
    public string ParentObjectName { get; set; } = default!;
    public byte[] Columns { get; set; } = new byte[] { };
    public string[] ColumnNames { get; set; } = new string[] { };
    public bool IsUnique { get; set; }
    public int RootNodePageId { get; set; }
}