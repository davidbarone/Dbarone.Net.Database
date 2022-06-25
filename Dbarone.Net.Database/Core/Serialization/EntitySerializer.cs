namespace Dbarone.Net.Database;

/// <summary>
/// Serializes and deserializes .NET objects to and from byte[] arrays.
/// </summary>
public class EntitySerializer
{
    private IBuffer _buffer = new BufferBase();

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

    public byte[] Serialize(object obj) {
        var columnInfo = GetColumnInfo(obj.GetType());
        return Serialize(columnInfo, obj);
    }

    /// <summary>
    /// Serialises an object to a byte array which can be stored on disk
    /// </summary>
    /// <param name="columns"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    public byte[] Serialize(IEnumerable<ColumnInfo> columns, object obj)
    {
        var MemoryStream = new MemoryStream();
        var fixedColumns = columns.Where(c => Types.GetByDataType(c.DataType).IsFixedLength).OrderBy(c => c.Order);
        var variableColumns = columns.Where(c => !Types.GetByDataType(c.DataType).IsFixedLength).OrderBy(c => c.Order);
        int index = 0;
        var props = obj.GetType().GetProperties();

        // Total columns
        int totalColumns = columns.Count();
        _buffer.Write(totalColumns, index);
        index = index + (Types.GetByDataType(DataType.Int16).Size);

        // Fixed length columns
        int fixedLengthColumns = fixedColumns.Count();
        _buffer.Write(fixedLengthColumns, index);
        index = index + (Types.GetByDataType(DataType.Int16).Size);

        // Fixed data length

        // Write all the fixed length columns
        foreach (var col in fixedColumns)
        {
            var prop = props.First(p => p.Name.Equals(col.ColumnName));
            var value = prop.GetValue(obj);
            _buffer.Write(value, index);
            index = index + (Types.GetByType(prop.PropertyType).Size);
        }

        // Number of variable length columns
        int variableLengthColumns = fixedColumns.Count();
        _buffer.Write(variableLengthColumns, index);
        index = index + (Types.GetByDataType(DataType.Int16).Size);

        // variable length offsets
        var variableLengthOffsetTable = index;
        index = index + (Types.GetByDataType(DataType.Int32)).Size;

        List<int> variableSizes = new List<int>();
        List<object> variableValues = new List<object>();
        int v = 0;
        foreach (var col in variableColumns)
        {
            var prop = props.First(p => p.Name.Equals(col.ColumnName));
            var value = prop.GetValue(obj);
            var size = Types.SizeOf(value);
            variableSizes.Add(size);
            variableValues.Add(value);
            _buffer.Write(value, index);
            index = index + size;
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
            _buffer.Write(offset, variableLengthColumns + (i * (Types.GetByDataType(DataType.Int32)).Size));
        }

        return _buffer.Slice(0, finalSize);
    }
   
}