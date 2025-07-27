using System.Data;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Dbarone.Net.Document;
using Microsoft.VisualBasic;

namespace Dbarone.Net.Database;

public class TableSerializer : ITableSerializer
{
    #region Public methods

    public (IBuffer Buffer, long Length) Serialize(Table table, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        var buf = new GenericBuffer();

        // Write data
        this.SerializeTable(buf, table, textEncoding);

        return (Buffer: buf, Length: buf.Position);
    }


    public (IBuffer Buffer, long Length) SerializeRow(TableRow row, TextEncoding textEncoding = TextEncoding.UTF8, TableSchema? schema = null)
    {
        IBuffer buffer = new GenericBuffer();
        SerializeRowInternal(buffer, row, textEncoding, schema);
        return (Buffer: buffer, Length: buffer.Length);
    }

    public (Table Table, List<byte[]> RowBuffers) Deserialize(IBuffer buffer, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        var result = this.DeserializeTable(buffer, textEncoding); // to do - add in schema
        return result;
    }

    #endregion

    #region Private Serialize Methods

    private void SerializeTable(GenericBuffer buffer, Table table, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        // Write schema
        if (table.Schema != null)
        {
            // Validate document first:
            if (table.IsValid)
            {
                // If got here, document is valid - we can write schema to serialised header.
                buffer.Write(1);   // Schema header present
                var schemaAsTable = table.Schema.ToTable();
                this.SerializeTable(buffer, schemaAsTable, textEncoding);
            }
        }
        else
        {
            buffer.Write(0);   // No schema header present
        }

        // write the row count
        var rows = table.Count();
        buffer.Write(rows);

        foreach (var row in table)
        {
            this.SerializeRowInternal(buffer, row, textEncoding, table.Schema);
        }
    }

    private void SerializeRowInternal(IBuffer buffer, TableRow row, TextEncoding textEncoding = TextEncoding.UTF8, TableSchema? schema = null)
    {
        if (row is null)
        {
            throw new Exception("row is null!");
        }
        var columns = row.Count();
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

    private void SerializeCell(IBuffer buffer, TableCell cell, TextEncoding textEncoding = TextEncoding.UTF8)
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

    private (Table Table, List<byte[]> RowBuffers) DeserializeTable(IBuffer buffer, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        Table table = new Table();
        var rowBuffers = new List<byte[]>();

        // Check for schema header
        var schemaFlag = buffer.ReadInt64();
        TableSchema? schema = null;

        if (schemaFlag != 0)
        {
            var schemaTable = this.DeserializeTable(buffer, textEncoding).Table;
            schema = TableSchema.FromTable(schemaTable);
        }

        long rows = buffer.ReadInt64();

        for (int i = 0; i < rows; i++)
        {
            var startPos = buffer.Position;
            var row = this.DeserializeRow(buffer, schema, textEncoding);
            var endPos = buffer.Position;
            var bytes = buffer.Slice((int)startPos, (int)endPos - (int)startPos);
            rowBuffers.Add(bytes);
            table.Add(row);
        }

        table.Schema = schema;
        return (Table: table, RowBuffers: rowBuffers);
    }

    private TableRow DeserializeRow(IBuffer buffer, TableSchema? schema = null, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        TableRow row = new TableRow();

        // First field is field-count
        var fields = buffer.ReadInt64();

        if (schema is null)
        {
            // no schema
            for (int i = 0; i < fields; i++)
            {
                // key
                string key = this.DeserializeCell(buffer, textEncoding);

                // value
                var value = this.DeserializeCell(buffer, textEncoding);
                row[key] = value;
            }
            return row;
        }
        else if (schema.Attributes is not null)
        {
            // has schema
            for (int i = 0; i < fields; i++)
            {
                // key
                short idx = this.DeserializeCell(buffer, textEncoding);
                string key = schema.Attributes.First(a => a.AttributeId == idx).AttributeName;
                var value = this.DeserializeCell(buffer, textEncoding);
                row[key] = value;
            }
            return row;
        }
        else
        {
            throw new Exception("Cannot deserialize. Schema attributes not set.");
        }
    }

    private TableCell DeserializeCell(IBuffer buffer, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        var i = buffer.ReadInt64();
        var serialType = new SerialType(i);
        switch (serialType.DocumentType)
        {
            case DocumentType.Null:
                return TableCell.Null;
            case DocumentType.Boolean:
                var boolValue = buffer.ReadBool();
                return new TableCell(boolValue);
            case DocumentType.Integer:
                var int64Value = buffer.ReadInt64();
                return new TableCell(int64Value);
            case DocumentType.Real:
                var doubleValue = buffer.ReadDouble();
                return new TableCell(doubleValue);
            case DocumentType.DateTime:
                var dateTimeValue = buffer.ReadDateTime();
                return new TableCell(dateTimeValue);
            case DocumentType.Text:
                var stringValue = buffer.ReadString(serialType.Length!.Value);
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