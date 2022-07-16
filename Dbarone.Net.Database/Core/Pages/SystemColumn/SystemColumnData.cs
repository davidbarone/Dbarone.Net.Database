namespace Dbarone.Net.Database;

/// <summary>
/// The item data for the SystemColumnPage type.
/// </summary>
public class SystemColumnData : PageData  {
    public string TableName { get; set; } = default!;
    public string ColumnName { get; set; } = default!;
    public int Order { get; set; }
    public DataType DataType { get; set; } = default!;
    public int? MaxLength { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool IsNullable { get; set; }    
}