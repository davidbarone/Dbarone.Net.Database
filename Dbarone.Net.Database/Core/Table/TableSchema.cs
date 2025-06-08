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
    public IEnumerable<SchemaAttribute>? Attributes { get; set; } = null;

    /// <summary>
    /// Creates a new SchemaElement instance.
    /// </summary>
    /// <param name="documentType">The document type.</param>
    /// <param name="allowNull">Set to true if TableCell.Null is allowed.</param>
    /// <param name="element">The inner element for TableCell.Array types.</param>
    /// <param name="attributes">The inner attributes for TableCell.Document types.</param>
    public SchemaElement(DocumentType documentType, bool allowNull, SchemaElement? element = null, IEnumerable<SchemaAttribute>? attributes = null)
    {
        this.DocumentType = documentType;
        this.AllowNull = allowNull;
        this.Element = element;
        this.Attributes = attributes;
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

    /// <summary>
    /// Validates a document using the current schema.
    /// </summary>
    /// <param name="document"></param>
    /// <returns></returns>
    public bool Validate(TableCell document)
    {
        if (!(this.DocumentType == document.Type || (this.AllowNull && document.IsNull)))
        {
            throw new Exception("Validation error: Invalid type.");
        }

        // Array?
        if (this.DocumentType == DocumentType.Array)
        {
            if (this.Element == null)
            {
                throw new Exception("Array validation: Element is null.");
            }
            foreach (var item in document.AsArray)
            {
                this.Element.Validate(item);
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
    /// This is defined as a document array, where each element
    /// in array is a schema-bound document, where each element
    /// in the document is a primitive type (i.e. not array or document) 
    /// </summary>
    /// <returns>Returns true if a tabular schema</returns>
    public bool IsTabularSchema()
    {
        return
            this.DocumentType == DocumentType.Array &&              // Must be document array
            this.Element != null &&                                 // Must have inner element defined
            this.Element.DocumentType == DocumentType.Document &&   // Inner element must be a dictionary document
            this.Element.Attributes != null &&                      // Inner dictionary document must have attributes defined
            this.Element.Attributes.Count() > 0 &&                  // Inner dictionary document must have attributes defined
            this.Element.Attributes.All(a => a.Element.DocumentType != DocumentType.Array && a.Element.DocumentType != DocumentType.Document);
        // All attributes must be primitive / document value types
    }

    /// <summary>
    /// Returns true if the schema describes a list of simple elements.
    /// This is defined as a document array, where each element
    /// in the array is a primitive TableCell type (i.e. not array
    /// or document) 
    /// </summary>
    /// <returns>Returns true if a tabular schema</returns>
    public bool IsListSchema()
    {
        return
            this.DocumentType == DocumentType.Array &&              // Must be document array
            this.Element != null &&                                 // Must have inner element defined
            this.Element.DocumentType != DocumentType.Array && this.Element.DocumentType != DocumentType.Document;
    }

    private static SchemaElement? GetCompatibleSchema(SchemaElement? el1, SchemaElement? el2)
    {
        SchemaElement result = new SchemaElement();
        if (el1 is null || el2 is null)
        {
            // If either schema is undefined, the resulting schema is undefined.
            return null;
        }
        else if (el1.DocumentType == DocumentType.Null && el2.DocumentType == DocumentType.Null)
        {
            result.DocumentType = DocumentType.Null;
            result.AllowNull = true;
            return result;
        }
        else if (el1.DocumentType == DocumentType.Array && el2.DocumentType == DocumentType.Array)
        {
            // 2 arrays. Only compatible if inner elements are compatible
            result.DocumentType = DocumentType.Array;
            result.AllowNull = false;
            result.Element = GetCompatibleSchema(el1.Element, el2.Element);
            return result;
        }
        else if (el1.DocumentType == DocumentType.Document && el2.DocumentType == DocumentType.Document)
        {
            // 2 documents. Only compatible if inner attributes are compatible
            result.DocumentType = DocumentType.Document;
            result.AllowNull = false;
            if (el1.Attributes is null || el2.Attributes is null)
            {
                // If either document schema is null, the 2 schemas are not compatible.
                result.Attributes = null;
                return result;
            }
            else if (el1.Attributes.Count() != el2.Attributes.Count())
            {
                // if the document schemas have different numbers of attributes, they are not compatible.
                result.Attributes = null;
                return result;
            }
            else if (el1.Attributes.Select(a => a.AttributeId).Except(el2.Attributes.Select(a => a.AttributeId)).Any())
            {
                // if the document schemas have different attribute ids, they are not compatible.
                result.Attributes = null;
                return result;
            }
            else
            {
                var attributes = new List<SchemaAttribute>();
                foreach (var attribute in el1.Attributes.OrderBy(a => a.AttributeId))
                {
                    // if at any attribute id, the names don't match, the attributes are incompatible
                    SchemaAttribute attr2 = el2.Attributes.First(a => a.AttributeId == attribute.AttributeId);
                    if (attribute.AttributeName != attr2.AttributeName)
                    {
                        attributes = null;
                        break;
                    }
                    var compatibleSchema = GetCompatibleSchema(attribute.Element, attr2.Element);
                    if (compatibleSchema is null)
                    {
                        // if the 2 attributes aren't compatible, neither are the entire documents.
                        attributes = null;
                        break;
                    }
                    else
                    {
                        SchemaAttribute attr = new SchemaAttribute(
                            attribute.AttributeId,
                            attribute.AttributeName,
                            compatibleSchema
                            );
                        attributes.Add(attr);
                    }
                }
                // At this point, if we have a list of attributes, it must be compatible.
                if (attributes is not null)
                {
                    result.Attributes = attributes;
                }
                return result;
            }
        }
        else if (el1.DocumentType == DocumentType.Null)
        {
            result.DocumentType = el2.DocumentType;
            result.AllowNull = true;
            result.Attributes = el2.Attributes;
            result.Element = el2.Element;
            return result;
        }
        else if (el2.DocumentType == DocumentType.Null)
        {
            result.DocumentType = el1.DocumentType;
            result.AllowNull = true;
            result.Attributes = el1.Attributes;
            result.Element = el1.Element;
            return result;
        }
        else if (el1.DocumentType == el2.DocumentType)
        {
            result.DocumentType = el1.DocumentType;
            result.AllowNull = el1.AllowNull == false && el2.AllowNull == false ? false : true;
            result.Attributes = el1.Attributes;
            result.Element = el1.Element;
            return result;
        }
        else
        {
            throw new Exception("Unable to get compatible type!");
        }
    }

    private static SchemaElement GetArraySchema(DocumentArray array)
    {
        SchemaElement schema = new SchemaElement();
        schema.DocumentType = DocumentType.Array;
        SchemaElement? elementSchema = null;
        foreach (var item in array)
        {
            if (elementSchema == null)
            {
                elementSchema = Parse(item);
            }
            else
            {
                // get compatible element
                elementSchema = GetCompatibleSchema(elementSchema, Parse(item));
                if (elementSchema is null)
                {
                    // not compatible
                    break;
                }
            }
        }
        schema.Element = elementSchema;
        return schema;
    }

    private static SchemaElement GetDictSchema(DictionaryDocument dict)
    {
        short attributeId = 0;
        List<SchemaAttribute> attributes = new List<SchemaAttribute>();
        foreach (var key in dict.Keys.OrderBy(k => k))
        {
            attributeId++;
            SchemaAttribute attr = new SchemaAttribute(attributeId, key, Parse(dict[key]));
            attributes.Add(attr);
        }
        return new SchemaElement(DocumentType.Document, false, null, attributes);
    }

    /// <summary>
    /// Generates a schema from parsing an existing document value.
    /// </summary>
    /// <param name="docValue">The document to generate the schema from.</param>
    /// <returns>Returns a document schema by parsing the document.</returns>
    public static SchemaElement Parse(TableCell docValue)
    {
        if (docValue.Type == DocumentType.Null)
        {
            return new SchemaElement(DocumentType.Null, false, null, null);
        }
        else if (docValue.Type == DocumentType.Array)
        {
            return GetArraySchema(docValue.AsArray);
        }
        else if (docValue.Type == DocumentType.Document)
        {
            return GetDictSchema(docValue.AsDocument);
        }
        else
        {
            return new SchemaElement(docValue.Type, false, null, null);
        }
    }
}
