using System.Data;
using System.Text.RegularExpressions;
using Dbarone.Net.Document;
using Microsoft.VisualBasic;

namespace Dbarone.Net.Database;

public class DocumentSerializer : IDocumentSerializer
{
    #region Serialize

    public void SerializeTable(GenericBuffer buffer, Table table, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        var rows = table.Count();
        // write the row count
        buffer.Write(new SerialType(DocumentType.Integer).Value);
        buffer.Write(rows);

        foreach (var row in table)
        {
            this.SerializeRow(buffer, row, textEncoding);
        }
    }

    public void SerializeRow(GenericBuffer buffer, TableRow row, TextEncoding textEncoding = TextEncoding.UTF8, TableSchema? schema = null)
    {
        if (row is null)
        {
            throw new Exception("row is null!");
        }
        var columns = row.Count();
        buffer.Write(new SerialType(DocumentType.Integer));
        buffer.Write(columns);

        // 2 ways of writing out dictionary documents - with/without attached schema
        // if schema is written to header, we write the column id + value for each column.
        // otherwise, we write out the column name + value for each column.

        // next byte: 100 = no separate schema, 101 = schema defined
        if (schema != null)
        {
            buffer.Write((byte)101);
        }
        else
        {
            buffer.Write((byte)100);
        }

        foreach (var key in row.Keys)
        {
            // key
            if (schema != null)
            {
                var idx = schema.Attributes!.First(a => a.AttributeName.Equals(key, StringComparison.Ordinal)).AttributeId;
                SerialType serialTypeKey = new SerialType(DocumentType.Integer);
                buffer.Write(serialTypeKey.Value);
                buffer.Write(idx);
            }
            else
            {
                var keyBytes = EncodeString(key, textEncoding);
                SerialType serialTypeKey = new SerialType(DocumentType.Text, keyBytes.Length);
                buffer.Write(serialTypeKey.Value);
                buffer.Write(keyBytes);
            }

            // value
            this.SerializeCell(buffer, row[key], textEncoding);
        }
    }

    public void SerializeCell(GenericBuffer buffer, TableCell cell, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        switch (cell.Type)
        {
            case DocumentType.Null:
                var serialType = new SerialType(DocumentType.Null);
                buffer.Write(serialType.Value);
                break;
            case DocumentType.Boolean:
                serialType = new SerialType(DocumentType.Boolean);
                buffer.Write(serialType.Value);
                buffer.Write(cell.AsBoolean);
                break;
            case DocumentType.Integer:
                serialType = new SerialType(DocumentType.Integer);
                buffer.Write(serialType.Value);
                buffer.Write(cell.AsInteger);
                break;
            case DocumentType.Real:
                serialType = new SerialType(DocumentType.Real);
                buffer.Write(serialType.Value);
                buffer.Write(cell.AsReal);
                break;
            case DocumentType.DateTime:
                serialType = new SerialType(DocumentType.DateTime);
                buffer.Write(serialType.Value);
                buffer.Write(cell.AsDateTime);
                break;
            case DocumentType.Text:
                var bytes = EncodeString(cell.AsText, textEncoding);
                serialType = new SerialType(DocumentType.Text, bytes.Length);
                buffer.Write(serialType.Value);
                buffer.Write(bytes);
                break;
            case DocumentType.Blob:
            default:
                throw new NotImplementedException();
        }
    }


    public byte[] Serialize(Table table, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        var buf = new GenericBuffer();

        // Magic byte
        buf.Write((byte)219);   // 0xDB

        // Write schema
        if (schema != null)
        {
            // Validate document first:
            schema.Validate(document);

            // If got here, document is valid - we can write schema to serialised header.
            buf.Write((byte)101);   // Schema header present
            this.Serialize(buf, schema.ToDictionaryDocument(), null, textEncoding);
        }
        else
        {
            buf.Write((byte)100);   // No schema header present
        }

        // Write data
        this.Serialize(buf, document, schema, textEncoding);

        // End byte
        buf.Write((byte)219);   // 0xDB

        return buf.ToArray();
    }

    #endregion

    #region Deserialize

    private TableRow DeserializeRow()
    {
            case DocumentType.Document:

            DictionaryDocument dict = new DictionaryDocument();

            // Next byte is schema flag
            byte schemaByte = buf.ReadByte();

            if (schemaByte == 100)
            {
                // no schema
                for (int i = 0; i < serialType.Length; i++)
                {
                    // key
                    string key = this.Deserialize(buf, null, textEncoding);
                    // value
                    var value = this.Deserialize(buf, null, textEncoding);
                    dict[key] = value;
                }
                return dict;
            }
            else if (schemaByte == 101)
            {
                if (schema is null)
                {
                    throw new Exception("Schema required to decode idx value");
                }

                if (schema.Attributes is null)
                {
                    throw new Exception("Schema attributes required to decode idx values");
                }
                // no schema
                for (int i = 0; i < serialType.Length; i++)
                {
                    // key
                    short idx = this.Deserialize(buf, schema, textEncoding);

                    SchemaElement? innerAttribute = null;
                    if (!(schema is null) && !(schema.Attributes is null))
                    {
                        string key = schema.Attributes.First(a => a.AttributeId == idx).AttributeName;
                        innerAttribute = schema.Attributes.First(a => a.AttributeName.Equals(key, StringComparison.Ordinal)).Element;
                        var value = this.Deserialize(buf, innerAttribute, textEncoding);
                        dict[key] = value;
                    }
                    else
                    {
                        throw new Exception("Invalid idx");
                    }
                }
                return dict;
            }
            else
            {
                throw new Exception("Unexpected Schema type byte mark");
            }

        }


    private TableCell DeserializeCell(GenericBuffer buf, SchemaElement? schema, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        var varInt = buf.ReadVarInt();
        var serialType = new SerialType(varInt);
        switch (serialType.DocumentType)
        {
            case DocumentType.Null:
                return TableCell.Null;
            case DocumentType.Boolean:
                var boolValue = buf.ReadBool();
                return new TableCell(boolValue);
            case DocumentType.Integer:
                var int64Value = buf.ReadInt64();
                return new TableCell(int64Value);
            case DocumentType.Real:
                var doubleValue = buf.ReadDouble();
                return new TableCell(doubleValue);
            case DocumentType.DateTime:
                var dateTimeValue = buf.ReadDateTime();
                return new TableCell(dateTimeValue);
            case DocumentType.Text:
                var stringValue = buf.ReadString(serialType.Length!.Value);
                return new TableCell(stringValue);
            default:
                throw new NotImplementedException();
        }
    }

    public void DeserializeTable()
    {
            case DocumentType.Array:
            List<DocumentValue> elements = new List<DocumentValue>();

            SchemaElement? innerElement = null;
            if (!(schema is null) && !(schema.Element is null))
            {
                innerElement = schema.Element;
            }
            for (int i = 0; i < serialType.Length; i++)
            {
                var docValue = this.Deserialize(buf, innerElement, textEncoding);
                elements.Add(docValue);
            }
            return new DocumentArray(elements);

        }

    public DocumentValue Deserialize(byte[] buffer, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        var buf = new GenericBuffer(buffer);

        // Magic bytes
        var headerByte1 = buf.ReadByte();   // 0xDB
        if (headerByte1 != 0xDB)
        {
            throw new Exception("Invalid serialization format.");
        }

        // Check for schema header
        var schemaByte = buf.ReadByte();
        SchemaElement? schema = null;

        if (schemaByte == 101)
        {
            var schemaDocument = this.Deserialize(buf, null, textEncoding).AsDocument;
            schema = new SchemaElement(schemaDocument);
        }

        DocumentValue result = this.Deserialize(buf, schema, textEncoding);

        // Magic footer byte
        var footerByte1 = buf.ReadByte();   // 0xDB
        if (footerByte1 != 0xDB)
        {
            throw new Exception("Invalid serialization format.");
        }

        return result;
    }

    #endregion

    #region Private Methods

    private byte[] EncodeString(string value, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        var buf = new GenericBuffer();
        // save string only - to get length in bytes
        var len = buf.Write(value);
        buf.Position = 0;
        var bytes = buf.ReadBytes(len);
        return bytes;
    }

    #endregion
}