namespace Dbarone.Net.Database;

/// <summary>
/// Represents an attribute node in a document schema definition.
/// </summary>
public class SchemaAttribute
{
    /// <summary>
    /// The attribute id. Attribute ids must NOT be changed on a type, as they are used to encode objects during the serialisation process.
    /// </summary>
    public short AttributeId { get; set; } = default!;

    /// <summary>
    /// The attribute name.
    /// </summary>
    public string AttributeName { get; set; } = default!;

    public DocumentType DocumentType { get; set; }

    public Boolean AllowNull { get; set; }

    public SchemaAttribute(short attributeId, string attributeName, DocumentType documentType, bool allowNull)
    {
        this.AttributeId = attributeId;
        this.AttributeName = attributeName;
        this.DocumentType = documentType;
        this.AllowNull = allowNull;
    }

    /// <summary>
    /// Creates a new SchemaAttribute from a <see cref="DictionaryDocument"/> instance.
    /// </summary>
    /// <param name="dictionaryDocument"></param>
    public SchemaAttribute(DictionaryDocument dictionaryDocument)
    {
        this.AttributeId = (short)dictionaryDocument["AttributeId"].AsInt16;
        this.AttributeName = dictionaryDocument["AttributeName"].AsString;
        this.Element = new SchemaElement(dictionaryDocument["Element"].AsDocument);
    }

    /// <summary>
    /// Converts the current SchemaAttribute to a <see cref="DictionaryDocument"/> instance.
    /// </summary>
    /// <returns></returns>
    public DictionaryDocument ToDictionaryDocument()
    {
        DictionaryDocument dd = new DictionaryDocument();
        dd["AttributeId"] = new TableCell(AttributeId);
        dd["AttributeName"] = new TableCell(AttributeName);
        dd["Element"] = Element.ToDictionaryDocument();
        return dd;
    }
}