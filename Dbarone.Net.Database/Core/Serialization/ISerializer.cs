using Dbarone.Net.Document;

namespace Dbarone.Net.Database;

/// <summary>
/// Defines serialization methods available.
/// </summary>
public interface ISerializer
{
    /// <summary>
    /// Serialises a document to a byte array.
    /// </summary>
    /// <param name="value">The document to be serialised.</param>
    /// <param name="textEncoding">The optional text encoding to use for serialisation.</param>
    /// <returns>Returns a serialised byte array representing the document.</returns>
    byte[] Serialize(DocumentValue value);

    DocumentValue Deserialize(byte[] buffer);

    public byte[] Serialize(object obj);

    public object Deserialize(byte[] buffer, Type toType);

    public PageBuffer Serialize(Page page);

    public Page Deserialize(PageBuffer buffer);

    public bool IsPageOverflow(Page page, DictionaryDocument? data = null, object? cell = null);
}