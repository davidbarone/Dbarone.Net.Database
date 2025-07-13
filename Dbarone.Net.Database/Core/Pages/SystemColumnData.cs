using Dbarone.Net.Document;

namespace Dbarone.Net.Database;

/// <summary>
/// The item data for the SystemColumnPage type.
/// </summary>
public class SystemColumnData
{
    public int ObjectId { get; set; }
    public string Name { get; set; } = default!;
    public DocumentType DocumentType { get; set; } = default!;
    public bool IsNullable { get; set; }

    public SystemColumnData(int objectId, string name, Type type)
    {
        /*
        this.ObjectId = objectId;
        this.Name = name;
        this.DocumentType = Types.GetByType(type).DataType;
        this.IsNullable = type.IsNullable();
        */

    }

    public SystemColumnData(int objectId, string name, DocumentType documentType, bool isNullable)
    {
        this.ObjectId = objectId;
        this.Name = name;
        this.DocumentType = documentType;
        this.IsNullable = isNullable;
    }
}