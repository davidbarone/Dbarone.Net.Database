namespace Dbarone.Net.Database;
using Dbarone.Net.Assertions;
using System.Collections;
using Dbarone.Net.Extensions.Object;

/// <summary>
/// Base class for Serializer. A serializer perform serialize and deserialize functions to convert .NET objects to and from byte[] arrays.
/// </summary>
public abstract class BaseSerializer : ISerializer
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
    public abstract byte[] SerializeDictionary(IEnumerable<ColumnInfo> columns, IDictionary<string, object?> obj, RowStatus rowStatus, TextEncoding textEncoding = TextEncoding.UTF8);

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

    public abstract DeserializationResult<IDictionary<string, object?>> DeserializeDictionary(IEnumerable<ColumnInfo> columns, byte[] buffer, TextEncoding textEncoding = TextEncoding.UTF8);

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
