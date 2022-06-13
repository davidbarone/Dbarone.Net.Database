public class ColumnInfo
{
    public string ColumnName { get; set; } = default!;
    public string TableName { get; set; } = default!;
    public int Order { get; set; }
    public Type DataType { get; set; } = default!;
    public int? MaxLength { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool IsNullable { get; set; }
}