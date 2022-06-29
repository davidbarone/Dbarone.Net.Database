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

        var fixedColumns = columns.Where(c => Types.GetByDataType(c.DataType).IsFixedLength).OrderBy(c => c.Order);
        var variableColumns = columns.Where(c => !Types.GetByDataType(c.DataType).IsFixedLength).OrderBy(c => c.Order);

        parms.TotalCount = (short)columns.Count();
        parms.FixedCount = (short)fixedColumns.Count();
        parms.VariableCount = (short)variableColumns.Count();
        parms.FixedSize = (short)fixedColumns.Sum(f => Types.GetByDataType(f.DataType).Size);
        parms.VariableSizes = new Dictionary<string, short>();
        foreach (var item in variableColumns){
            parms.VariableSizes.Add(item.ColumnName, )
        }


        List<short> variableSizes = new List<short>();
        List<object> variableValues = new List<object>();
        foreach (var col in variableColumns)
        {
            var prop = props.First(p => p.Name.Equals(col.ColumnName));
            var value = prop.GetValue(obj);
            var size = Types.SizeOf(value);
            variableSizes.Add(size);
            variableValues.Add(value);
            buffer.Write(value, index);
            index += size;
        }
        // Final size
        var finalSize = index;

        // Go back and write the variable offsets table
        for (int i = 0; i < variableSizes.Count(); i++)
        {
            var offset = 0;
            for (int j = 0; j < i; j++)
            {
                offset += variableSizes[j];
            }
            //buffer.Write(offset, variableLengthOffsetTable + (i * (Types.GetByDataType(DataType.Int32)).Size));
            buffer.Write(variableSizes[i], variableLengthOffsetTable + (i * (Types.GetByDataType(DataType.Int16)).Size));
        }

        return buffer.Slice(0, finalSize);


    }

    /// <summary>
    /// Serialises an object to a byte array which can be stored on disk
    /// </summary>
    /// <param name="columns"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    public byte[] Serialize(IEnumerable<ColumnInfo> columns, object obj)
    {
        var buffer = new BufferBase();

        var fixedColumns = columns.Where(c => Types.GetByDataType(c.DataType).IsFixedLength).OrderBy(c => c.Order);
        var variableColumns = columns.Where(c => !Types.GetByDataType(c.DataType).IsFixedLength).OrderBy(c => c.Order);
        short index = 0;
        var props = obj.GetType().GetProperties();

        // Total columns
        short totalColumns = (short)columns.Count();
        buffer.Write(totalColumns, index);
        index += (Types.GetByDataType(DataType.Int16).Size);

        // Fixed length columns
        int fixedLengthColumns = (short)fixedColumns.Count();
        buffer.Write(fixedLengthColumns, index);
        index += (Types.GetByDataType(DataType.Int16).Size);

        // Fixed data length
        index += Types.GetByDataType(DataType.Int16).Size;
        var fixedDataLengthOffset = index;

        // Write all the fixed length columns
        foreach (var col in fixedColumns)
        {
            var prop = props.First(p => p.Name.Equals(col.ColumnName));
            var value = prop.GetValue(obj);
            buffer.Write(value, index);
            index += (Types.GetByType(prop.PropertyType).Size);
        }

        // write the fixed data length
        buffer.Write((short)(index - fixedDataLengthOffset), 4);

        // Number of variable length columns
        short variableLengthColumns = (short)variableColumns.Count();
        buffer.Write(variableLengthColumns, index);
        index += (Types.GetByDataType(DataType.Int16).Size);

        // variable length offsets
        var variableLengthOffsetTable = index;
        index += (short)(variableLengthColumns * (Types.GetByDataType(DataType.Int16)).Size);

        List<short> variableSizes = new List<short>();
        List<object> variableValues = new List<object>();
        foreach (var col in variableColumns)
        {
            var prop = props.First(p => p.Name.Equals(col.ColumnName));
            var value = prop.GetValue(obj);
            var size = Types.SizeOf(value);
            variableSizes.Add(size);
            variableValues.Add(value);
            buffer.Write(value, index);
            index += size;
        }
        // Final size
        var finalSize = index;

        // Go back and write the variable offsets table
        for (int i = 0; i < variableSizes.Count(); i++)
        {
            var offset = 0;
            for (int j = 0; j < i; j++)
            {
                offset += variableSizes[j];
            }
            //buffer.Write(offset, variableLengthOffsetTable + (i * (Types.GetByDataType(DataType.Int32)).Size));
            buffer.Write(variableSizes[i], variableLengthOffsetTable + (i * (Types.GetByDataType(DataType.Int16)).Size));
        }

        return buffer.Slice(0, finalSize);
    }
}