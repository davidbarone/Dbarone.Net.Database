namespace Dbarone.Net.Database;

public class ColumnInfo
{
    public string ColumnName { get; set; } = default!;
    public int Order { get; set; }
    public DataType DataType { get; set; } = default!;
    public int? MaxLength { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool IsNullable { get; set; }
}