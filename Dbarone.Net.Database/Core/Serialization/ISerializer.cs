namespace Dbarone.Net.Database;

/// <summary>
/// Defines serialization methods available.
/// </summary>
public interface ISerializer
{
    #region Serialization

    byte[] Serialize(object obj, RowStatus rowStatus, TextEncoding textEncoding = TextEncoding.UTF8);
    byte[] Serialize(IEnumerable<ColumnInfo> columns, object obj, RowStatus rowStatus, TextEncoding textEncoding = TextEncoding.UTF8);
    byte[] SerializeDictionary(IEnumerable<ColumnInfo> columns, IDictionary<string, object?> obj, RowStatus rowStatus, TextEncoding textEncoding = TextEncoding.UTF8);

    #endregion

    #region Deserialization

    DeserializationResult<T> Deserialize<T>(byte[] buffer, TextEncoding textEncoding = TextEncoding.UTF8);
    DeserializationResult<object> Deserialize(Type type, byte[] buffer, TextEncoding textEncoding = TextEncoding.UTF8);
    DeserializationResult<T> Deserialize<T>(IEnumerable<ColumnInfo> columns, byte[] buffer, TextEncoding textEncoding = TextEncoding.UTF8) where T : IPageData;
    DeserializationResult<object> Deserialize(Type type, IEnumerable<ColumnInfo> columns, byte[] buffer, TextEncoding textEncoding = TextEncoding.UTF8);
    DeserializationResult<IDictionary<string, object?>> DeserializeDictionary(IEnumerable<ColumnInfo> columns, byte[] buffer, TextEncoding textEncoding = TextEncoding.UTF8);

    #endregion

    RowStatus GetRowStatus(byte[] buffer);
    IEnumerable<ColumnInfo> GetColumnsForType(Type type);
}