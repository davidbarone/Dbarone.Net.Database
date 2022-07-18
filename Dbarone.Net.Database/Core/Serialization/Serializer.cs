namespace Dbarone.Net.Database;
using Dbarone.Net.Assertions;

/// <summary>
/// Serializes and deserializes .NET objects to and from byte[] arrays.
/// </summary>
public class Serializer
{
    /// <summary>
    /// Gets the column information for an entity type.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static IEnumerable<ColumnInfo> GetColumnInfo(Type type)
    {
        List<ColumnInfo> columns = new List<ColumnInfo>();
        var properties = type.GetProperties();
        int i = 0;
        foreach (var property in properties)
        {
            TypeInfo t = default!;
            if (property.PropertyType.IsEnum) {
                t = Types.GetByType(Enum.GetUnderlyingType(property.PropertyType));
            } else {
                t = Types.GetByType(property.PropertyType);
            }
            columns.Add(new ColumnInfo()
            {
                ColumnName = property.Name,
                DataType = t.DataType,
                MaxLength = t.Size,
                IsNullable = false,
                Order = i++,
                IsPrimaryKey = false
            });
        }
        return columns;
    }

    public static T Deserialize<T>(byte[] buffer)
    {
        return (T)Deserialize(typeof(T), buffer);
    }

    public static object Deserialize(Type type, byte[] buffer)
    {
        var columns = GetColumnInfo(type);

        object? obj = Activator.CreateInstance(type);
        if (obj == null)
        {
            throw new Exception("Error creating object");
        }
        var fixedColumns = columns.Where(c => Types.GetByDataType(c.DataType).IsFixedLength).OrderBy(c => c.Order);
        var variableColumns = columns.Where(c => !Types.GetByDataType(c.DataType).IsFixedLength).OrderBy(c => c.Order).ToList();
        var props = obj.GetType().GetProperties();

        IBuffer bb = new BufferBase(buffer);
        var totalColumns = bb.ReadUInt16(0);
        var bufferLength = bb.ReadUInt16(2);
        var totalLength = bb.ReadUInt16(4);

        Assert.Equals((ushort)buffer.Length, bufferLength);

        var fixedLengthColumns = bb.ReadUInt16(6);
        ushort fixedLength = bb.ReadUInt16(8);
        ushort startFixedLength = 10;
        ushort index = startFixedLength;

        foreach (var col in fixedColumns)
        {
            var prop = props.First(p => p.Name.Equals(col.ColumnName));
            var value = bb.Read(col.DataType, index);
            prop.SetValue(obj, value);
            index += Types.GetByDataType(col.DataType).Size;
        }

        // Check fixed data length is correct
        Assert.Equals((ushort)(index - startFixedLength), fixedLength);

        // number of variable length columns
        var variableLengthCount = bb.ReadUInt16(index);
        index += (Types.GetByDataType(DataType.UInt16).Size);
        List<ushort> variableLengthLengths = new List<ushort>(variableLengthCount);
        for (var i = 0; i < variableLengthCount; i++)
        {
            variableLengthLengths.Add(bb.ReadUInt16(index));
            index += (Types.GetByDataType(DataType.UInt16).Size);
        }

        for (var i = 0; i < variableLengthCount; i++)
        {
            var col = variableColumns[i];
            var value = bb.Read(col.DataType, index, variableLengthLengths[i]);
            var prop = props.First(p => p.Name.Equals(col.ColumnName));
            prop.SetValue(obj, value);
            index += variableLengthLengths[i];
        }

        Assert.Equals(totalColumns, (ushort)(fixedLengthColumns + variableLengthCount));

        return obj;
    }

    private static SerializationParams GetParams(IEnumerable<ColumnInfo> columns, object obj)
    {
        SerializationParams parms = new SerializationParams();
        var props = obj.GetType().GetProperties();

        var fixedColumns = columns.Where(c => Types.GetByDataType(c.DataType).IsFixedLength).OrderBy(c => c.Order);
        var variableColumns = columns.Where(c => !Types.GetByDataType(c.DataType).IsFixedLength).OrderBy(c => c.Order);

        parms.TotalCount = (ushort)columns.Count();
        parms.FixedCount = (ushort)fixedColumns.Count();
        parms.VariableCount = (ushort)variableColumns.Count();
        parms.FixedSize = (ushort)fixedColumns.Sum(f => Types.GetByDataType(f.DataType).Size);
        parms.FixedColumns = new List<ColumnSerializationInfo>();
        foreach (var item in fixedColumns)
        {
            ColumnSerializationInfo csi = new ColumnSerializationInfo();
            var prop = props.First(p => p.Name.Equals(item.ColumnName));
            csi.ColumnName = item.ColumnName;
            csi.Value = prop.GetValue(obj);
            csi.Size = Types.GetByType(prop.PropertyType).Size;
            parms.FixedColumns.Add(csi);
        }
        parms.VariableColumns = new List<ColumnSerializationInfo>();
        foreach (var item in variableColumns)
        {
            ColumnSerializationInfo csi = new ColumnSerializationInfo();
            var prop = props.First(p => p.Name.Equals(item.ColumnName));
            csi.ColumnName = item.ColumnName;
            csi.Value = prop.GetValue(obj);
            csi.Size = Types.SizeOf(csi.Value);
            parms.VariableColumns.Add(csi);
        }
        parms.VariableSize = (ushort)parms.VariableColumns.Sum(c => c.Size);
        parms.TotalSize = (ushort)(parms.FixedSize + parms.VariableSize);

        return parms;
    }

    /// <summary>
    /// Serialises an object to a byte array.
    /// </summary>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>A byte array representing the object.</returns>
    public static byte[] Serialize(object obj)
    {
        var columnInfo = GetColumnInfo(obj.GetType());
        return Serialize(columnInfo, obj);
    }

    /// <summary>
    /// Serialises an object to a byte array.
    /// </summary>
    /// <param name="columns">The column metadata for the object to serialize.</param>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>A byte array representing the object.</returns>
    public static byte[] Serialize(IEnumerable<ColumnInfo> columns, object obj)
    {
        // Get serialization parameters
        var parms = GetParams(columns, obj);

        // Additional info stored
        // Total Columns (2 bytes)
        // BufferLength (2 bytes)     // includes metadata fields
        // Total Length (2 bytes)   // Just size of data
        // Fixed Length Columns (2 bytes)
        // Fixed Length size (2 bytes)
        // Variable Length Columns (2 bytes)
        // Variable Length Table (2 bytes * variable columns)
        var ushortSize = Types.GetByDataType(DataType.UInt16).Size;
        var bufferSize = parms.TotalSize + (6 * ushortSize) + (ushortSize * parms.VariableCount);
        var buffer = new BufferBase(new byte[bufferSize]);

        ushort index = 0;

        // Total columns
        buffer.Write(parms.TotalCount, index);
        index += (Types.GetByDataType(DataType.UInt16).Size);

        // Buffer size
        buffer.Write(bufferSize, index);
        index += (Types.GetByDataType(DataType.UInt16).Size);

        // Total size
        buffer.Write(parms.TotalSize, index);
        index += (Types.GetByDataType(DataType.UInt16).Size);

        // Fixed length columns
        buffer.Write(parms.FixedCount, index);
        index += (Types.GetByDataType(DataType.UInt16).Size);

        // Fixed data length
        buffer.Write(parms.FixedSize, index);
        index += Types.GetByDataType(DataType.UInt16).Size;

        // Write all the fixed length columns
        var fixedDataLengthOffset = index;
        foreach (var item in parms.FixedColumns)
        {
            buffer.Write(item.Value, index);
            index += item.Size;
        }

        // Number of variable length columns
        buffer.Write(parms.VariableCount, index);
        index += (Types.GetByDataType(DataType.UInt16).Size);

        // variable length offsets
        for (var i = 0; i < parms.VariableCount; i++)
        {
            buffer.Write(parms.VariableColumns[i].Size, index);
            index += (Types.GetByDataType(DataType.UInt16).Size);
        }

        // variable values
        foreach (var item in parms.VariableColumns)
        {
            buffer.Write(item.Value, index);
            index += item.Size;
        }

        return buffer.ToArray();
    }
}