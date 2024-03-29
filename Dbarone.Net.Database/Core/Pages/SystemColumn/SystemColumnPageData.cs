namespace Dbarone.Net.Database;
using Dbarone.Net.Extensions.Reflection;

/// <summary>
/// The item data for the SystemColumnPage type.
/// </summary>
public class SystemColumnPageData : PageData
{
    public int ObjectId { get; set; }
    public string Name { get; set; } = default!;
    public DataType DataType { get; set; } = default!;
    public bool IsNullable { get; set; }

    public SystemColumnPageData(int objectId, string name, Type type)
    {
        this.ObjectId = objectId;
        this.Name = name;
        this.DataType = Types.GetByType(type).DataType;
        this.IsNullable = type.IsNullable();
    }

    public SystemColumnPageData(int objectId, string name, DataType dataType, bool isNullable)
    {
        this.ObjectId = objectId;
        this.Name = name;
        this.DataType = dataType;
        this.IsNullable = isNullable;
    }

    public SystemColumnPageData()
    {

    }

}