namespace Dbarone.Net.Database;

public class TypeInfo
{

    public DataType DataType { get; set; }
    public Type Type { get; set; }
    public sbyte Size { get; set; }
    public bool IsFixedLength => Size >= 0;

    public TypeInfo(DataType dataType, Type type, sbyte size)
    {
        this.Size = size;
        this.Type = type;
        this.DataType = dataType;
    }
}