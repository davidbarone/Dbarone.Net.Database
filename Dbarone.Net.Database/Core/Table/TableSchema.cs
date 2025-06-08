using System.Runtime.InteropServices;
using System.Xml.Schema;
using Dbarone.Net.Document;

namespace Dbarone.Net.Database;

/// <summary>
/// Represents an element in a document schema definition.
/// </summary>
public class TableSchema
{
    /// <summary>
    /// Specifies the attributes.
    /// </summary>
    public IEnumerable<SchemaAttribute> Attributes { get; set; } = new List<SchemaAttribute>();

    public void AddAttribute(string attributeName, DocumentType documentType, bool allowNull)
    {
        var attributeId = (short)this.Attributes.Count();
        var schemaAttribute = new SchemaAttribute(
            attributeId,
            attributeName,
            documentType,
            allowNull);
    }

    /// <summary>
    /// Creates a new SchemaElement instance from a <see cref="DictionaryDocument"/> instance. Used to deserlialise schemas.
    /// </summary>
    /// <param name="dictionaryDocument">The DictionaryDocument instance to create the SchemaElement from.</param>
    public SchemaElement(DictionaryDocument dictionaryDocument)
    {
        this.DocumentType = (DocumentType)dictionaryDocument["DocumentType"].AsInt32;
        this.AllowNull = dictionaryDocument["AllowNull"].AsBoolean;
        if (dictionaryDocument.ContainsKey("Element"))
        {
            this.Element = new SchemaElement(dictionaryDocument["Element"].AsDocument);
        }
        if (dictionaryDocument.ContainsKey("Attributes"))
        {
            this.Attributes = dictionaryDocument["Attributes"].AsArray.Select(a => new SchemaAttribute(a.AsDocument));
        }
    }

    /// <summary>
    /// Converts the current schema element to a <see cref="DictionaryDocument"/> instance. This is
    /// useful when the schema needs to be serialised.
    /// </summary>
    /// <returns></returns>
    public DictionaryDocument ToDictionaryDocument()
    {
        if (this.DocumentType == DocumentType.Array)
        {
            if (Element is null)
            {
                throw new Exception("Element cannot be null for DocumentType.Array.");
            }
            DictionaryDocument dd = new DictionaryDocument();
            dd["DocumentType"] = new TableCell((int)DocumentType);
            dd["AllowNull"] = new TableCell(AllowNull);
            dd["Element"] = Element.ToDictionaryDocument();
            return dd;
        }
        else if (this.DocumentType == DocumentType.Document)
        {
            if (Attributes is null)
            {
                throw new Exception("Attributes cannot be null for DocumentType.Document");
            }
            DictionaryDocument dd = new DictionaryDocument();
            dd["DocumentType"] = new TableCell((int)DocumentType);
            dd["AllowNull"] = new TableCell(AllowNull);
            dd["Attributes"] = new DocumentArray(Attributes.Select(a => a.ToDictionaryDocument()));
            return dd;
        }
        else
        {
            return new DictionaryDocument(new Dictionary<string, TableCell> {
                {"DocumentType", new TableCell((int)DocumentType)},
                {"AllowNull", new TableCell(AllowNull)}
            });
        }
    }

    public bool ValidateRow(TableRow row)
    {

    }

    public bool ValidateTable(Table table)
    {
        foreach (var item in table)
        {
            this.Element.Validate(item);
        }

    }

    /// <summary>
    /// Validates a document using the current schema.
    /// </summary>
    /// <param name="document"></param>
    /// <returns></returns>
    public bool Validate(TableCell document)
    {

        // Array?
        if (this.DocumentType == DocumentType.Array)
        {
            if (this.Element == null)
            {
                throw new Exception("Array validation: Element is null.");
            }
        }

        // Document?
        if (this.DocumentType == DocumentType.Document)
        {
            if (this.Attributes == null)
            {
                throw new Exception("Document validation: Attributes are null.");
            }

            var schemaAttributes = this.Attributes.Select(a => a.AttributeName);
            var dd = document.AsDocument;
            var documentAttributes = dd.Keys;

            // attributes can be missing in document if the schema AllowNull is set.
            var attributesMissingInDocument = schemaAttributes
                .Except(documentAttributes)
                .Where(
                        a => this.Attributes.First(f => f.AttributeName.Equals(a, StringComparison.Ordinal)).Element.AllowNull
                );
            var attributesMissingInSchema = documentAttributes.Except(schemaAttributes);

            if (attributesMissingInDocument.Any())
            {
                throw new Exception($"Attribute {attributesMissingInDocument.First()} is not defined in the document.");
            }

            if (attributesMissingInSchema.Any())
            {
                throw new Exception($"Attribute {attributesMissingInSchema.First()} is not defined in the schema.");
            }

            var validAttributes = schemaAttributes.Intersect(documentAttributes);

            // validate attributes
            foreach (var attribute in validAttributes)
            {
                var innerDocument = dd[attribute];
                var innerSchema = this.Attributes.First(a => a.AttributeName.Equals(attribute, StringComparison.Ordinal)).Element;
                innerSchema.Validate(innerDocument);
            }
        }

        // If get here, then all good.
        return true;
    }

    /// <summary>
    /// Returns true if the schema describes a tabular document.
    /// </summary>
    /// <returns>Returns true if a tabular schema</returns>
    public bool IsTabularSchema() => this.Attributes.Count() > 1;

    /// <summary>
    /// Returns true if the schema contains 1 column only.
    /// </summary>
    /// <returns>Returns true if a list schema</returns>
    public bool IsListSchema() => this.Attributes.Count() == 1;

}
