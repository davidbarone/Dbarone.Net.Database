using System.Data;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Dbarone.Net.Document;
using Microsoft.VisualBasic;

namespace Dbarone.Net.Database;

public class TableSerializer : ITableSerializer
{
    #region Public methods

    public byte[] Serialize(Table table, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        var buf = new GenericBuffer();

        // Magic byte
        buf.Write((byte)219);   // 0xDB

        // Write schema
        if (table.Schema != null)
        {
            // Validate document first:
            table.Schema.ValidateTable(table);

            // If got here, document is valid - we can write schema to serialised header.
            buf.Write(1);   // Schema header present
            this.SerializeTable(buf, table.Schema.ToTable(), textEncoding);
        }
        else
        {
            buf.Write(0);   // No schema header present
        }

        // Write data
        this.SerializeTable(buf, table, textEncoding);

        // End byte
        buf.Write((byte)219);   // 0xDB

        return buf.ToArray();
    }

    public Table Deserialize(byte[] buffer, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        var buf = new GenericBuffer(buffer);

        // Magic bytes
        var headerByte1 = buf.ReadInt64();   // 0xDB
        if (headerByte1 != 0xDB)
        {
            throw new Exception("Invalid serialization format.");
        }

        // Check for schema header
        var hasSchema = buf.ReadBool();
        TableSchema? schema = null;

        if (hasSchema)
        {
            var schemaTable = this.DeserializeTable(buf, textEncoding);
            schema = TableSchema.FromTable(schemaTable);
        }

        Table table = this.DeserializeTable(buf, textEncoding); // to do - add in schema

        // Magic footer byte
        var footerByte1 = buf.ReadInt64();   // 0xDB
        if (footerByte1 != 0xDB)
        {
            throw new Exception("Invalid serialization format.");
        }

        return table;
    }

    #endregion

    #region Private Serialize Methods

    private void SerializeTable(GenericBuffer buffer, Table table, TextEncoding textEncoding = TextEncoding.UTF8)
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

    private void SerializeRow(GenericBuffer buffer, TableRow row, TextEncoding textEncoding = TextEncoding.UTF8, TableSchema? schema = null)
    {
        if (row is null)
        {
            throw new Exception("row is null!");
        }
        var columns = row.Count();
        buffer.Write(new SerialType(DocumentType.Integer).Value);
        buffer.Write(columns);

        // 2 ways of writing out dictionary documents - with/without attached schema
        // if schema is written to header, we write the column id + value for each column.
        // otherwise, we write out the column name + value for each column.
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

    private void SerializeCell(GenericBuffer buffer, TableCell cell, TextEncoding textEncoding = TextEncoding.UTF8)
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

    #endregion

    #region Private Deserialize Methods

    private Table DeserializeTable(GenericBuffer buf, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        Table table = new Table();

        // 1st field is schema flag
        long hasSchema = buf.ReadInt64();
        long rows = buf.ReadInt64();

        for (int i = 0; i < rows; i++)
        {
            var row = this.DeserializeRow(buf, hasSchema, textEncoding);
            table.Add(row);
        }
        return table;
    }

    private TableRow DeserializeRow(GenericBuffer buf, long schemaFlag, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        TableRow row = new TableRow();

        // First field is field-count
        var fields = buf.ReadInt64();

        if (schemaFlag == 0)
        {
            // no schema
            for (int i = 0; i < fields; i++)
            {
                // key
                string key = this.DeserializeCell(buf, textEncoding);

                // value
                var value = this.DeserializeCell(buf, textEncoding);
                row[key] = value;
            }
            return row;
        }
        else
        {
            // has schema
            for (int i = 0; i < fields; i++)
            {
                // key
                short idx = 123;
                TableSchema schema = new TableSchema(); // to do...

                if (!(schema is null) && !(schema.Attributes is null))
                {
                    string key = schema.Attributes.First(a => a.AttributeId == idx).AttributeName;
                    var innerAttribute = schema.Attributes.First(a => a.AttributeName.Equals(key, StringComparison.Ordinal));
                    var value = this.DeserializeCell(buf, textEncoding);
                    row[key] = value;
                }
                else
                {
                    throw new Exception("Invalid idx");
                }
            }
            return row;
        }
    }

    private TableCell DeserializeCell(GenericBuffer buf, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        var i = buf.ReadInt64();
        var serialType = new SerialType(i);
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

    #endregion

    #region Private Methods

    private byte[] EncodeString(string value, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        var buf = new GenericBuffer();
        // save string only - to get length in bytes
        buf.Write(value, textEncoding);
        int len = (int)buf.Position; // get length of bytes written.
        buf.Position = 0;   // reset
        var bytes = buf.ReadBytes(len);
        return bytes;
    }

    #endregion
}