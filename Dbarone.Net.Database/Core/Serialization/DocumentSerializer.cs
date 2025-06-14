using System.Data;
using Dbarone.Net.Document;
using Microsoft.VisualBasic;

namespace Dbarone.Net.Database;

public class DocumentSerializer : IDocumentSerializer
{
    #region Serialize

    private void Serialize(GenericBuffer buffer, Table document, SchemaElement? schema = null, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        switch (document.Type)
        {
            case DocumentType.Null:
                var serialType = new SerialType(DocumentType.Null);
                buffer.Write(serialType.Value);
                break;
            case DocumentType.Boolean:
                serialType = new SerialType(DocumentType.Boolean);
                buffer.Write(serialType.Value);
                buffer.Write(document.AsBoolean);
                break;
            case DocumentType.Byte:
                serialType = new SerialType(DocumentType.Byte);
                buffer.Write(serialType.Value);
                buffer.Write(document.AsByte);
                break;
            case DocumentType.SByte:
                serialType = new SerialType(DocumentType.SByte);
                buffer.Write(serialType.Value);
                buffer.Write(document.AsSByte);
                break;
            case DocumentType.Char:
                serialType = new SerialType(DocumentType.Char);
                buffer.Write(serialType.Value);
                buffer.Write(document.AsChar);
                break;
            case DocumentType.DateTime:
                serialType = new SerialType(DocumentType.DateTime);
                buffer.Write(serialType.Value);
                buffer.Write(document.AsDateTime);
                break;
            case DocumentType.Decimal:
                serialType = new SerialType(DocumentType.Decimal);
                buffer.Write(serialType.Value);
                buffer.Write(document.AsDecimal);
                break;
            case DocumentType.Double:
                serialType = new SerialType(DocumentType.Double);
                buffer.Write(serialType.Value);
                buffer.Write(document.AsDouble);
                break;
            case DocumentType.Guid:
                serialType = new SerialType(DocumentType.Guid);
                buffer.Write(serialType.Value);
                buffer.Write(document.AsGuid);
                break;
            case DocumentType.Int16:
                serialType = new SerialType(DocumentType.Int16);
                buffer.Write(serialType.Value);
                buffer.Write((short)document.AsInt16);
                break;
            case DocumentType.Int32:
                serialType = new SerialType(DocumentType.Int32);
                buffer.Write(serialType.Value);
                buffer.Write(document.AsInt32);
                break;
            case DocumentType.Int64:
                serialType = new SerialType(DocumentType.Int64);
                buffer.Write(serialType.Value);
                buffer.Write(document.AsInt64);
                break;
            case DocumentType.UInt16:
                serialType = new SerialType(DocumentType.UInt16);
                buffer.Write(serialType.Value);
                buffer.Write((ushort)document.AsUInt16);
                break;
            case DocumentType.UInt32:
                serialType = new SerialType(DocumentType.UInt32);
                buffer.Write(serialType.Value);
                buffer.Write(document.AsUInt32);
                break;
            case DocumentType.UInt64:
                serialType = new SerialType(DocumentType.UInt64);
                buffer.Write(serialType.Value);
                buffer.Write(document.AsUInt64);
                break;
            case DocumentType.Single:
                serialType = new SerialType(DocumentType.Single);
                buffer.Write(serialType.Value);
                buffer.Write(document.AsSingle);
                break;
            case DocumentType.String:
                var bytes = EncodeString(document.AsString, textEncoding);
                serialType = new SerialType(DocumentType.String, bytes.Length);
                buffer.Write(serialType.Value);
                buffer.Write(bytes);
                break;
            case DocumentType.Array:
                var docArray = document as DocumentArray;
                if (docArray is null)
                {
                    throw new Exception("DocumentArray type expected!");
                }
                serialType = new SerialType(DocumentType.Array, docArray.Count);
                buffer.Write(serialType.Value);
                SchemaElement? innerElement = null;
                if (!(schema is null) && !(schema.Element is null))
                {
                    innerElement = schema.Element;
                }
                foreach (var docElement in docArray)
                {
                    this.Serialize(buffer, docElement, innerElement, textEncoding);
                }
                break;
            case DocumentType.Document:
                var dictDoc = document as DictionaryDocument;
                if (dictDoc is null)
                {
                    throw new Exception("DictionaryDocument type expected!");
                }
                serialType = new SerialType(DocumentType.Document, dictDoc.Keys.Count);
                buffer.Write(serialType.Value);

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

                foreach (var key in dictDoc.Keys)
                {
                    // key
                    if (schema != null)
                    {
                        var idx = schema.Attributes!.First(a => a.AttributeName.Equals(key, StringComparison.Ordinal)).AttributeId;
                        SerialType serialTypeKey = new SerialType(DocumentType.Int16);
                        buffer.Write(serialTypeKey.Value);
                        buffer.Write(idx);
                    }
                    else
                    {
                        var keyBytes = EncodeString(key, textEncoding);
                        SerialType serialTypeKey = new SerialType(DocumentType.String, keyBytes.Length);
                        buffer.Write(serialTypeKey.Value);
                        buffer.Write(keyBytes);
                    }

                    // value
                    this.Serialize(buffer, dictDoc[key], schema, textEncoding);
                }
                break;
            case DocumentType.Blob:
            case DocumentType.VarInt:
                serialType = new SerialType(DocumentType.VarInt);
                buffer.Write(serialType.Value);
                buffer.Write(document.AsVarInt);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    public byte[] Serialize(DocumentValue document, SchemaElement? schema = null, TextEncoding textEncoding = TextEncoding.UTF8)
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

    private DocumentValue Deserialize(GenericBuffer buf, SchemaElement? schema, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        var varInt = buf.ReadVarInt();
        var serialType = new SerialType(varInt);
        switch (serialType.DocumentType)
        {
            case DocumentType.Null:
                return DocumentValue.Null;
            case DocumentType.Boolean:
                var boolValue = buf.ReadBool();
                return new DocumentValue(boolValue);
            case DocumentType.Byte:
                var byteValue = buf.ReadByte();
                return new DocumentValue(byteValue);
            case DocumentType.SByte:
                var sByteValue = buf.ReadByte();
                return new DocumentValue(sByteValue);
            case DocumentType.Char:
                var charValue = buf.ReadChar();
                return new DocumentValue(charValue);
            case DocumentType.DateTime:
                var dateTimeValue = buf.ReadDateTime();
                return new DocumentValue(dateTimeValue);
            case DocumentType.Decimal:
                var decimalValue = buf.ReadDecimal();
                return new DocumentValue(decimalValue);
            case DocumentType.Double:
                var doubleValue = buf.ReadDouble();
                return new DocumentValue(doubleValue);
            case DocumentType.Guid:
                var guidValue = buf.ReadGuid();
                return new DocumentValue(guidValue);
            case DocumentType.Int16:
                var int16Value = buf.ReadInt16();
                return new DocumentValue(int16Value);
            case DocumentType.Int32:
                var int32Value = buf.ReadInt32();
                return new DocumentValue(int32Value);
            case DocumentType.Int64:
                var int64Value = buf.ReadInt64();
                return new DocumentValue(int64Value);
            case DocumentType.UInt16:
                var uint16Value = buf.ReadUInt16();
                return new DocumentValue(uint16Value);
            case DocumentType.UInt32:
                var uint32Value = buf.ReadUInt32();
                return new DocumentValue(uint32Value);
            case DocumentType.UInt64:
                var uint64Value = buf.ReadUInt64();
                return new DocumentValue(uint64Value);
            case DocumentType.Single:
                var singleValue = buf.ReadSingle();
                return new DocumentValue(singleValue);
            case DocumentType.String:
                var stringValue = buf.ReadString(serialType.Length!.Value);
                return new DocumentValue(stringValue);
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
            default:
                throw new NotImplementedException();
        }
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