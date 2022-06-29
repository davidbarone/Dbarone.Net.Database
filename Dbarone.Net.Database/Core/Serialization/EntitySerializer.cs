namespace Dbarone.Net.Database;
using Dbarone.Net.Assertions;

/// <summary>
/// Serializes and deserializes .NET objects to and from byte[] arrays.
/// </summary>
public class EntitySerializer
{
    public EntitySerializer()
    {

    }

    /// <summary>
    /// Gets the column information for an entity type.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public IEnumerable<ColumnInfo> GetColumnInfo(Type type)
    {
        List<ColumnInfo> columns = new List<ColumnInfo>();
        var properties = type.GetProperties();
        int i = 0;
        foreach (var property in properties)
        {
            var t = Types.GetByType(property.PropertyType);
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

    public byte[] Serialize(object obj)
    {
        var columnInfo = GetColumnInfo(obj.GetType());
        return Serialize(columnInfo, obj);
    }

    public T Deserialize<T>(IEnumerable<ColumnInfo> columns, byte[] buffer)
    {
        T obj = Activator.CreateInstance<T>();
        if (obj == null)
        {
            throw new Exception("Error creating object");
        }
        var fixedColumns = columns.Where(c => Types.GetByDataType(c.DataType).IsFixedLength).OrderBy(c => c.Order);
        var variableColumns = columns.Where(c => !Types.GetByDataType(c.DataType).IsFixedLength).OrderBy(c => c.Order).ToList();
        var props = obj.GetType().GetProperties();

        IBuffer bb = new BufferBase(buffer);
        var totalColumns = bb.ReadInt16(0);
        var fixedLengthColumns = bb.ReadInt16(2);
        short fixedLength = bb.ReadInt16(4);
        short startFixedLength = 6;
        short index = startFixedLength;

        foreach (var col in fixedColumns)
        {
            var prop = props.First(p => p.Name.Equals(col.ColumnName));
            var value = bb.Read(col.DataType, index);
            prop.SetValue(obj, value);
            index += Types.GetByDataType(col.DataType).Size;
        }

        // Check fixed data length is correct
        Assert.Equals((short)(index - startFixedLength), fixedLength);

        // number of variable length columns
        var variableLengthCount = bb.ReadInt16(index);
        index += (Types.GetByDataType(DataType.Int16).Size);
        List<short> variableLengthLengths = new List<short>(variableLengthCount);
        for (var i = 0; i < variableLengthCount; i++)
        {
            variableLengthLengths.Add(bb.ReadInt16(index));
            index += (Types.GetByDataType(DataType.Int16).Size);
        }

        for (var i = 0; i < variableLengthCount; i++)
        {
            var col = variableColumns[i];
            var value = bb.Read(col.DataType, index, variableLengthLengths[i]);
            var prop = props.First(p => p.Name.Equals(col.ColumnName));
            prop.SetValue(obj, value);
            index += variableLengthLengths[i];
        }

        Assert.Equals(totalColumns, (short)(fixedLengthColumns + variableLengthCount));

        return (T)obj;
    }

    public SerializationParams GetParams(IEnumerable<ColumnInfo> columns, object obj)
    {
        SerializationParams parms = new SerializationParams();
        var props = obj.GetType().GetProperties();

        var fixedColumns = columns.Where(c => Types.GetByDataType(c.DataType).IsFixedLength).OrderBy(c => c.Order);
        var variableColumns = columns.Where(c => !Types.GetByDataType(c.DataType).IsFixedLength).OrderBy(c => c.Order);

        parms.TotalCount = (short)columns.Count();
        parms.FixedCount = (short)fixedColumns.Count();
        parms.VariableCount = (short)variableColumns.Count();
        parms.FixedSize = (short)fixedColumns.Sum(f => Types.GetByDataType(f.DataType).Size);
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
        parms.VariableSize = (short)parms.VariableColumns.Sum(c => c.Size);
        parms.TotalSize = (short)(parms.FixedSize + parms.VariableSize);

        return parms;
    }

    /// <summary>
    /// Serialises an object to a byte array which can be stored on disk
    /// </summary>
    /// <param name="columns"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    public byte[] Serialize(IEnumerable<ColumnInfo> columns, object obj)
    {
        // Get serialization parameters
        var parms = GetParams(columns, obj);

        // Additional info stored
        // Total Columns (2 bytes)
        // Fixed Length Columns (2 bytes)
        // Fixed Length size (2 bytes)
        // Variable Length Columns (2 bytes)
        // Variable Length Table (2 bytes * variable columns)
        var shortSize = Types.GetByDataType(DataType.UInt16).Size;
        var bufferSize = parms.TotalSize + (4 * shortSize) + (shortSize * parms.VariableCount);
        var buffer = new BufferBase(new byte[bufferSize]);

        short index = 0;

        // Total columns
        buffer.Write(parms.TotalCount, index);
        index += (Types.GetByDataType(DataType.Int16).Size);

        // Fixed length columns
        buffer.Write(parms.FixedCount, index);
        index += (Types.GetByDataType(DataType.Int16).Size);

        // Fixed data length
        buffer.Write(parms.FixedSize, index);
        index += Types.GetByDataType(DataType.Int16).Size;

        // Write all the fixed length columns
        var fixedDataLengthOffset = index;
        foreach (var item in parms.FixedColumns)
        {
            buffer.Write(item.Value, index);
            index += item.Size;
        }

        // Number of variable length columns
        buffer.Write(parms.VariableCount, index);
        index += (Types.GetByDataType(DataType.Int16).Size);

        // variable length offsets
        for (var i = 0; i < parms.VariableCount; i++)
        {
            buffer.Write(parms.VariableColumns[i].Size, index);
            index += (Types.GetByDataType(DataType.Int16).Size);
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