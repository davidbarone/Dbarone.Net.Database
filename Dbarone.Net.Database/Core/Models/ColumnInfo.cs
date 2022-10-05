namespace Dbarone.Net.Database;
using Dbarone.Net.Extensions.Reflection;

public class ColumnInfo
{
    public string Name { get; set; } = default!;
    //public int Order { get; set; }
    public DataType DataType { get; set; } = default!;
    //public int? MaxLength { get; set; }
    //public bool IsPrimaryKey { get; set; }
    public bool IsNullable { get; set; }

    public ColumnInfo(string name, Type type)
    {
        this.Name = name;
        this.DataType = Types.GetByType(type).DataType;
        this.IsNullable = type.IsNullable();
    }

    public ColumnInfo(string name, DataType dataType, bool isNullable)
    {
        this.Name = name;
        this.DataType = dataType;
        this.IsNullable = isNullable;
    }

    public ColumnInfo() {
        
    }
}