namespace Dbarone.Net.Database;
using Dbarone.Net.Assertions;
using System.Collections;
using Dbarone.Net.Extensions.Object;

/// <summary>
/// Serializes and deserializes .NET objects to and from byte[] arrays.
/// </summary>
public class Serializer
{
    public static DeserializationResult<T> Deserialize<T>(byte[] buffer, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        var result = Deserialize(typeof(T), buffer, textEncoding);
        return new DeserializationResult<T>((T?)result.Result, result.RowStatus);
    }

    public static DeserializationResult<object> Deserialize(Type type, byte[] buffer, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        var columns = GetColumnsForType(type);
        return Deserialize(type, columns, buffer, textEncoding);
    }

    public static DeserializationResult<T> Deserialize<T>(IEnumerable<ColumnInfo> columns, byte[] buffer, TextEncoding textEncoding = TextEncoding.UTF8) where T : IPageData
    {
        var result = Deserialize(typeof(T), columns, buffer, textEncoding);
        return new DeserializationResult<T>((T?)result.Result, result.RowStatus);
    }

    public static DeserializationResult<object> Deserialize(Type type, IEnumerable<ColumnInfo> columns, byte[] buffer, TextEncoding textEncoding = TextEncoding.UTF8)
    {

        var result = DeserializeDictionary(columns, buffer, textEncoding);

        if (result.RowStatus.HasFlag(RowStatus.Deleted) | result.RowStatus.HasFlag(RowStatus.Null)) {
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

    /// <summary>
    /// Inspects a buffer, and extracts the row status.
    /// </summary>
    /// <param name="buffer"></param>
    public static RowStatus GetRowStatus(byte[] buffer)
    {
        IBuffer bb = new BufferBase(buffer);
        var rowStatus = (RowStatus)bb.ReadByte(4);
        return rowStatus;
    }

    public static DeserializationResult<IDictionary<string, object?>> DeserializeDictionary(IEnumerable<ColumnInfo> columns, byte[] buffer, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        Dictionary<string, object?>? obj = null;

        var fixedColumns = columns.Where(c => Types.GetByDataType(c.DataType).IsFixedLength).ToList();
        var variableColumns = columns.Where(c => !Types.GetByDataType(c.DataType).IsFixedLength).ToList();

        IBuffer bb = new BufferBase(buffer);
        var bufferLength = bb.ReadUInt32(0);
        var rowStatus = (RowStatus)bb.ReadByte(4);

        if (rowStatus.HasFlag(RowStatus.Deleted) || rowStatus.HasFlag(RowStatus.Null))
        {
            // no need to deserialize anything.
        }
        else
        {
            obj = new Dictionary<string, object?>();
            var totalLength = bb.ReadUInt32(5);
            var totalColumns = bb.ReadByte(9);

            byte columnIndex = 0;

            // Set up null bitmap
            var nullBitmap = new BitArray(totalColumns);
            var nullBitmapBytes = (byte)Math.Ceiling((double)totalColumns / 8);
            nullBitmap = new BitArray(bb.ReadBytes(10, nullBitmapBytes));

            Assert.Equals((uint)buffer.Length, bufferLength);

            var fixedLengthColumns = bb.ReadByte(10 + nullBitmapBytes);
            ushort fixedLength = bb.ReadUInt16(11 + nullBitmapBytes);
            ushort startFixedLength = (ushort)(13 + nullBitmapBytes);
            int index = startFixedLength;

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

            // Check fixed data length is correct
            Assert.Equals((int)(index - startFixedLength), (int)fixedLength);

            // number of variable length columns
            var variableLengthCount = bb.ReadByte(index);
            index += (Types.GetByDataType(DataType.Byte).Size);
            List<uint> variableLengthLengths = new List<uint>(variableLengthCount);
            for (var i = 0; i < variableLengthCount; i++)
            {
                variableLengthLengths.Add(bb.ReadUInt32(index));
                index += (Types.GetByDataType(DataType.UInt32).Size);
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
                    var value = bb.Read(col.DataType, index, (int)variableLengthLengths[i], textEncoding);
                    obj[col.Name] = value;
                }

                index += (int)variableLengthLengths[i];
                columnIndex++;
            }

            Assert.Equals(totalColumns, (byte)(fixedLengthColumns + variableLengthCount));
            Assert.Equals(columnIndex, totalColumns);
        }
        return new DeserializationResult<IDictionary<string, object?>>(obj, rowStatus);
    }

    /// <summary>
    /// Serialises an object to a byte array.
    /// </summary>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>A byte array representing the object.</returns>
    public static byte[] Serialize(object obj, RowStatus rowStatus, TextEncoding textEncoding = TextEncoding.UTF8)
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
    public static byte[] Serialize(IEnumerable<ColumnInfo> columns, object obj, RowStatus rowStatus, TextEncoding textEncoding = TextEncoding.UTF8)
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
    public static byte[] SerializeDictionary(IEnumerable<ColumnInfo> columns, IDictionary<string, object?> obj, RowStatus rowStatus, TextEncoding textEncoding = TextEncoding.UTF8)
    {

        // Get serialization parameters
        var parms = GetParams(rowStatus, columns, obj, textEncoding);

        // Additional info stored
        // BufferLength (2 bytes)     // includes metadata fields
        // Row status (1 byte)
        // Total Length (2 bytes)     // Just size of data
        // Total Columns (2 bytes)
        // Null bitmap (total columns / 8 bytes)
        // Fixed Length Columns (2 bytes)
        // Fixed Length size (2 bytes)
        // Variable Length Columns (2 bytes)
        // Variable Length Table (2 bytes * variable columns)

        var uintSize = Types.GetByDataType(DataType.UInt32).Size;
        var ushortSize = Types.GetByDataType(DataType.UInt16).Size;
        var byteSize = Types.GetByDataType(DataType.Byte).Size;
        byte columnIndex = 0;
        byte nullBitmapBytes;
        uint bufferSize;
        BufferBase buffer;
        int index = 0;

        BitArray nullBitmap;
        if (parms == null)
        {
            // For null / deleted records, we just store the buffer length (4 bytes) + rowStatus (1 byte)
            bufferSize = (uint)(uintSize + byteSize);
            buffer = new BufferBase(new byte[bufferSize]);
            // Buffer size
            buffer.Write(bufferSize, 0);
            index += (Types.GetByDataType(DataType.UInt32).Size);

            // Row status
            buffer.Write(rowStatus, index);
        }
        else
        {
            nullBitmap = new BitArray(parms.TotalCount);
            nullBitmapBytes = (byte)Math.Ceiling((double)parms.TotalCount / 8);
            bufferSize = (uint)(parms.TotalSize + (2 * uintSize) + (1 * ushortSize) + (4 * byteSize) + (parms.VariableCount * uintSize) + nullBitmapBytes + byteSize);
            buffer = new BufferBase(new byte[bufferSize]);

            // Buffer size
            buffer.Write(bufferSize, index);
            index += (Types.GetByDataType(DataType.UInt32).Size);

            // Row status
            buffer.Write(rowStatus, index);
            index += (Types.GetByDataType(DataType.Byte).Size);

            // Total size
            buffer.Write(parms.TotalSize, index);
            index += (Types.GetByDataType(DataType.UInt32).Size);

            // Total columns
            buffer.Write(parms.TotalCount, index);
            index += (Types.GetByDataType(DataType.Byte).Size);

            // Null Bitmap
            // Pad space - fill in later
            index += nullBitmapBytes;

            // Fixed length columns
            buffer.Write(parms.FixedCount, index);
            index += (Types.GetByDataType(DataType.Byte).Size);

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
                index += (int)item.Size;
                columnIndex++;
            }

            // Number of variable length columns
            buffer.Write(parms.VariableCount, index);
            index += (Types.GetByDataType(DataType.Byte).Size);

            // variable length offsets
            for (var i = 0; i < parms.VariableCount; i++)
            {
                buffer.Write(parms.VariableColumns[i].Size, index);
                index += (Types.GetByDataType(DataType.UInt32).Size);
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
                index += (int)item.Size;
                columnIndex++;
            }

            Assert.Equals(columnIndex, parms.TotalCount);

            // Write null bitmap
            var nullBitmapByteArray = new byte[nullBitmapBytes];
            nullBitmap.CopyTo(nullBitmapByteArray, 0);
            buffer.Write(nullBitmapByteArray, 10);   // bitmap array starts at offset 10

        }
        return buffer.ToArray();
    }

    /// <summary>
    /// Returns the columns for a particular type based on the public properties.
    /// </summary>
    /// <param name="type">The type to return columns for.</param>
    /// <returns>A list of columns for the type.</returns>
    public static IEnumerable<ColumnInfo> GetColumnsForType(Type type)
    {
        List<ColumnInfo> result = new List<ColumnInfo>();
        var properties = type.GetProperties();
        foreach (var property in properties)
        {
            result.Add(new ColumnInfo(property.Name, property.PropertyType));
        }
        return result;
    }

    #region Private Methods

    private static SerializationParams? GetParams(RowStatus rowStatus, IEnumerable<ColumnInfo> columns, IDictionary<string, object?> obj, TextEncoding textEncoding = TextEncoding.UTF8)
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

            parms.TotalCount = (byte)columns.Count();
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
            parms.VariableSize = (uint)parms.VariableColumns.Sum(c => c.Size);
            parms.TotalSize = (uint)(parms.FixedSize + parms.VariableSize);

            return parms;
        }
    }

    #endregion
}
