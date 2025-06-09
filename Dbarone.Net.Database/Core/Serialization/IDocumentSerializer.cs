namespace Dbarone.Net.Database;

/// <summary>
/// Interface for serialising and deserialising DocumentValue objects.
/// </summary>
public interface IDocumentSerializer
{
    /// <summary>
    /// Serialise a <see cref="Table"/> instance. 
    /// </summary>
    /// <param name="table">The instance to serialise.</param>
    /// <param name="textEncoding">The optional text encoding to use.</param>
    /// <returns>Returns a byte array.</returns>
    byte[] Serialize(Table table, TextEncoding textEncoding = TextEncoding.UTF8);

    /// <summary>
    /// Deserialise a buffer to a <see cref="Table"/> instance.
    /// </summary>
    /// <param name="buffer">The buffer byte array</param>
    /// <param name="textEncoding">Optional text encoding to use.</param>
    /// <returns>Returns a Table instance.</returns>
    Table Deserialize(byte[] buffer, TextEncoding textEncoding = TextEncoding.UTF8);
}