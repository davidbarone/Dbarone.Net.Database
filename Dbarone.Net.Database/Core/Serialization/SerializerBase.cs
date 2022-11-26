namespace Dbarone.Net.Database;
using Dbarone.Net.Assertions;
using System.Collections;
using Dbarone.Net.Extensions.Object;

/// <summary>
/// Serializes and deserializes .NET objects to and from byte[] arrays.
/// </summary>
public class SerializerBase : ISerializer
{
    #region Serialisation

    /// <summary>
    /// Serialises an object to a byte array.
    /// </summary>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>A byte array representing the object.</returns>
    public byte[] Serialize(object obj, RowStatus rowStatus, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        var columnInfo = GetColumnsForType(obj.GetType());
        return Serialize(columnInfo, obj, rowStatus, textEncoding);
    }

    /// <summary>
    /// Serialises an object to a byte array.
    /// </summary>
    /// <param name="columns">The column metadata for the object to serialize.</param>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>A byte array representing the object.</returns>
    public byte[] Serialize(IEnumerable<ColumnInfo> columns, object obj, RowStatus rowStatus, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        var dict = obj.ToDictionary();
        return SerializeDictionary(columns, dict!, rowStatus, textEncoding);
    }

    /// <summary>
    /// Serializes a dictionary.
    /// </summary>
    /// <param name="columns">The column metadata.</param>
    /// <param name="obj">The object / dictionary to serialize.</param>
    /// <returns>A byte array of the serialized data.</returns>
    public virtual byte[] SerializeDictionary(IEnumerable<ColumnInfo> columns, IDictionary<string, object?> obj, RowStatus rowStatus, TextEncoding textEncoding = TextEncoding.UTF8)
    {

        // Get serialization parameters
        var parms = GetParams(rowStatus, columns, obj, textEncoding);

        // Additional info stored
        // BufferLength (4 bytes)
        // Row status (1 byte)
        // Fixed Length Column Count (1 byte)
        // Variable Length Column Count (1 bytes)
        // Null bitmap ((Fixed Length Column Count + Variable Length Column Count) / 8 bytes)
        // Fixed Length size (2 bytes)
        // Variable Length Table (4 bytes * variable columns)

        var intSize = Types.GetByDataType(DataType.Int32).Size;
        var ushortSize = Types.GetByDataType(DataType.UInt16).Size;
        var byteSize = Types.GetByDataType(DataType.Byte).Size;
        byte columnIndex = 0;
        byte nullBitmapBytes;
        int bufferSize;
        BufferBase buffer;
        int index = 0;

        BitArray nullBitmap;
        if (parms == null)
        {
            // For null / deleted records, we just store the buffer length (4 bytes) + rowStatus (1 byte)
            bufferSize = intSize + byteSize;
            buffer = new BufferBase(new byte[bufferSize]);
            // Buffer size
            buffer.Write(bufferSize, 0);
            index += (Types.GetByDataType(DataType.Int32).Size);

            // Row status
            buffer.Write(rowStatus, index);
        }
        else
        {
            nullBitmap = new BitArray(parms.TotalCount);
            nullBitmapBytes = (byte)Math.Ceiling((double)parms.TotalCount / 8);
            bufferSize = (int)(parms.TotalSize + (2 * intSize) + (1 * ushortSize) + (4 * byteSize) + (parms.VariableCount * intSize) + nullBitmapBytes + byteSize);
            buffer = new BufferBase(new byte[bufferSize]);

            // Buffer size
            buffer.Write(bufferSize, index);
            index += (Types.GetByDataType(DataType.UInt32).Size);

            // Row status
            buffer.Write(rowStatus, index);
            index += (Types.GetByDataType(DataType.Byte).Size);

            // Fixed length columns
            buffer.Write(parms.FixedCount, index);
            index += (Types.GetByDataType(DataType.Byte).Size);

            // Number of variable length columns
            buffer.Write(parms.VariableCount, index);
            index += (Types.GetByDataType(DataType.Byte).Size);

            // Null Bitmap
            // Pad space - fill in later
            index += nullBitmapBytes;

            // Fixed data length
            buffer.Write(parms.FixedSize, index);
            index += Types.GetByDataType(DataType.UInt16).Size;

            // Write all the fixed length columns
            var fixedDataLengthOffset = index;
            foreach (var item in parms.FixedColumns)
            {
                if (item.Value == null || !obj.ContainsKey(item.ColumnName))
                {
                    nullBitmap[columnIndex] = true;
                }
                else
                {
                    buffer.Write(item.Value, index);
                }
                index += item.Size;
                columnIndex++;
            }

            // variable length offsets
            for (var i = 0; i < parms.VariableCount; i++)
            {
                buffer.Write(parms.VariableColumns[i].Size, index);
                index += (Types.GetByDataType(DataType.Int32).Size);
            }

            // variable values
            foreach (var item in parms.VariableColumns)
            {
                if (item.Value == null || !obj.ContainsKey(item.ColumnName))
                {
                    nullBitmap[columnIndex] = true;
                }
                else
                {
                    buffer.Write(item.Value, index, textEncoding);
                }
                index += item.Size;
                columnIndex++;
            }

            Assert.Equals(columnIndex, parms.TotalCount);

            // Write null bitmap
            var nullBitmapByteArray = new byte[nullBitmapBytes];
            nullBitmap.CopyTo(nullBitmapByteArray, 0);
            buffer.Write(nullBitmapByteArray, 7);   // bitmap array starts at offset 7

        }
        return buffer.ToArray();
    }

    #endregion

    #region Deserialisation

    public DeserializationResult<T> Deserialize<T>(byte[] buffer, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        var result = Deserialize(typeof(T), buffer, textEncoding);
        return new DeserializationResult<T>((T?)result.Result, result.RowStatus);
    }

    public DeserializationResult<object> Deserialize(Type type, byte[] buffer, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        var columns = GetColumnsForType(type);
        return Deserialize(type, columns, buffer, textEncoding);
    }

    public DeserializationResult<T> Deserialize<T>(IEnumerable<ColumnInfo> columns, byte[] buffer, TextEncoding textEncoding = TextEncoding.UTF8) where T : IPageData
    {
        var result = Deserialize(typeof(T), columns, buffer, textEncoding);
        return new DeserializationResult<T>((T?)result.Result, result.RowStatus);
    }

    public DeserializationResult<object> Deserialize(Type type, IEnumerable<ColumnInfo> columns, byte[] buffer, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        var result = DeserializeDictionary(columns, buffer, textEncoding);

        if (result.RowStatus.HasFlag(RowStatus.Deleted) | result.RowStatus.HasFlag(RowStatus.Null))
        {
            return new DeserializationResult<object>((object?)null, result.RowStatus);
        }

        object? obj = Activator.CreateInstance(type);
        if (obj == null)
        {
            throw new Exception("Error creating object");
        }
        var props = obj.GetType().GetProperties();

        foreach (var column in columns)
        {
            props.First(p => p.Name.Equals(column.Name)).SetValue(obj, result.Result[column.Name]);
        }
        return new DeserializationResult<object>(obj, result.RowStatus);
    }

    public virtual DeserializationResult<IDictionary<string, object?>> DeserializeDictionary(IEnumerable<ColumnInfo> columns, byte[] buffer, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        Dictionary<string, object?>? obj = null;

        var fixedColumns = columns.Where(c => Types.GetByDataType(c.DataType).IsFixedLength).ToList();
        var variableColumns = columns.Where(c => !Types.GetByDataType(c.DataType).IsFixedLength).ToList();

        IBuffer bb = new BufferBase(buffer);
        var bufferLength = bb.ReadInt32(0);
        Assert.Equals(buffer.Length, bufferLength);

        var rowStatus = (RowStatus)bb.ReadByte(4);

        if (rowStatus.HasFlag(RowStatus.Deleted) || rowStatus.HasFlag(RowStatus.Null))
        {
            // no need to deserialize anything.
        }
        else
        {
            obj = new Dictionary<string, object?>();
            var fixedLengthCount = bb.ReadByte(5);
            var variableLengthCount = bb.ReadByte(6);
            var totalColumnCount = (byte)(fixedLengthCount + variableLengthCount);

            // Set up null bitmap
            var nullBitmap = new BitArray(totalColumnCount);
            var nullBitmapBytes = (byte)Math.Ceiling((double)totalColumnCount / 8);
            nullBitmap = new BitArray(bb.ReadBytes(7, nullBitmapBytes));

            ushort fixedLength = bb.ReadUInt16(7 + nullBitmapBytes);
            ushort startFixedLength = (ushort)(9 + nullBitmapBytes);
            int index = startFixedLength;
            byte columnIndex = 0;

            // Fixed length columns
            foreach (var col in fixedColumns)
            {
                if (nullBitmap[columnIndex] == true)
                {
                    obj[col.Name] = null;
                }
                else
                {
                    var value = bb.Read(col.DataType, index);
                    obj[col.Name] = value;
                }

                index += Types.GetByDataType(col.DataType).Size;
                columnIndex++;
            }

            Assert.Equals(index - startFixedLength, (int)fixedLength);

            // Variable length columns
            List<int> variableLengthLengths = new List<int>(variableLengthCount);
            for (var i = 0; i < variableLengthCount; i++)
            {
                variableLengthLengths.Add(bb.ReadInt32(index));
                index += (Types.GetByDataType(DataType.Int32).Size);
            }

            for (var i = 0; i < variableLengthCount; i++)
            {
                var col = variableColumns[i];

                if (nullBitmap[columnIndex] == true)
                {
                    obj[col.Name] = null;
                }
                else
                {
                    var value = bb.Read(col.DataType, index, variableLengthLengths[i], textEncoding);
                    obj[col.Name] = value;
                }

                index += variableLengthLengths[i];
                columnIndex++;
            }

            Assert.Equals(totalColumnCount, (byte)(fixedLengthCount + variableLengthCount));
            Assert.Equals(columnIndex, totalColumnCount);
        }
        return new DeserializationResult<IDictionary<string, object?>>(obj, rowStatus);
    }

    #endregion

    #region Other public methods

    /// <summary>
    /// Inspects a buffer, and extracts the row status.
    /// </summary>
    /// <param name="buffer"></param>
    public RowStatus GetRowStatus(byte[] buffer)
    {
        IBuffer bb = new BufferBase(buffer);
        var rowStatus = (RowStatus)bb.ReadByte(4);
        return rowStatus;
    }

    /// <summary>
    /// Returns the columns for a particular type based on the public properties.
    /// </summary>
    /// <param name="type">The type to return columns for.</param>
    /// <returns>A list of columns for the type.</returns>
    public IEnumerable<ColumnInfo> GetColumnsForType(Type type)
    {
        List<ColumnInfo> result = new List<ColumnInfo>();
        var properties = type.GetProperties().OrderBy(p => p.Name);
        foreach (var property in properties)
        {
            result.Add(new ColumnInfo(property.Name, property.PropertyType));
        }
        return result;
    }

    #endregion

    #region Protected Methods

    protected SerializationParams? GetParams(RowStatus rowStatus, IEnumerable<ColumnInfo> columns, IDictionary<string, object?> obj, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        SerializationParams parms = new SerializationParams();
        if (rowStatus.HasFlag(RowStatus.Deleted) || rowStatus.HasFlag(RowStatus.Null))
        {
            return null;
        }
        else
        {
            var fixedColumns = columns.Where(c => Types.GetByDataType(c.DataType).IsFixedLength);
            var variableColumns = columns.Where(c => !Types.GetByDataType(c.DataType).IsFixedLength);

            parms.FixedCount = (byte)fixedColumns.Count();
            parms.VariableCount = (byte)variableColumns.Count();
            parms.FixedSize = (ushort)fixedColumns.Sum(f => Types.GetByDataType(f.DataType).Size);
            parms.FixedColumns = new List<ColumnSerializationInfo>();
            foreach (var item in fixedColumns)
            {
                ColumnSerializationInfo csi = new ColumnSerializationInfo();
                csi.ColumnName = item.Name;
                csi.Value = obj.ContainsKey(item.Name) ? obj[item.Name] : null;
                csi.Size = Types.GetByDataType(item.DataType).Size;
                parms.FixedColumns.Add(csi);
            }
            parms.VariableColumns = new List<ColumnSerializationInfo>();
            foreach (var item in variableColumns)
            {
                ColumnSerializationInfo csi = new ColumnSerializationInfo();
                csi.ColumnName = item.Name;
                csi.Value = obj.ContainsKey(item.Name) ? obj[item.Name] : null;
                if (csi.Value != null)
                {
                    csi.Size = Types.SizeOf(csi.Value, textEncoding);
                }
                parms.VariableColumns.Add(csi);
            }
            parms.VariableSize = parms.VariableColumns.Sum(c => c.Size);
            return parms;
        }
    }

    #endregion
}
