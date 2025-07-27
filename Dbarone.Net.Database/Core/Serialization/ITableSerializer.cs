namespace Dbarone.Net.Database;

/// <summary>
/// Interface for serialising and deserialising Table objects.
/// </summary>
public interface ITableSerializer
{
    /// <summary>
    /// Serialise a <see cref="Table"/> instance. 
    /// </summary>
    /// <param name="table">The instance to serialise.</param>
    /// <param name="textEncoding">The optional text encoding to use.</param>
    /// <returns>Returns a byte array.</returns>
    (IBuffer Buffer, long Length) Serialize(Table table, TextEncoding textEncoding = TextEncoding.UTF8);

    (IBuffer Buffer, long Length) SerializeRow(TableRow row, TextEncoding textEncoding = TextEncoding.UTF8, TableSchema? schema = null);

    /// <summary>
    /// Deserialise a buffer to a <see cref="Table"/> instance.
    /// </summary>
    /// <param name="buffer">The buffer byte array</param>
    /// <param name="textEncoding">Optional text encoding to use.</param>
    /// <returns>Returns a Table instance.</returns>
    (Table Table, List<byte[]> RowBuffers) Deserialize(IBuffer buffer, TextEncoding textEncoding = TextEncoding.UTF8);
}