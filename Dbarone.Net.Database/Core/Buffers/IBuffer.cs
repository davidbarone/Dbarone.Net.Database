using Dbarone.Net.Document;

namespace Dbarone.Net.Database;

/// <summary>
/// Describes operations that can be performed on a buffer.
/// </summary>
public interface IBuffer_TO_BE_DELETED_COPY_OF_SERIALISATION_ONE
{
    /// <summary>
    /// Clears bytes in the buffer
    /// </summary>
    /// <param name="index"></param>
    /// <param name="length"></param>
    public void Clear(int index, int length);

    /// <summary>
    /// Fills the buffer with a byte.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="length"></param>
    /// <param name="value"></param>
    public void Fill(int index, int length, byte value);

    /// <summary>
    /// Returns a byte array representation of the buffer.
    /// </summary>
    /// <returns></returns>
    public byte[] ToArray();

    /// <summary>
    /// Returns a slice of the byte array.
    /// </summary>
    /// <param name="index">The start of the byte array to return.</param>
    /// <param name="length">The length of the byte array to return.</param>
    /// <returns></returns>
    public byte[] Slice(int index, int length);

    /// <summary>
    /// Gets the size of the buffer.
    /// </summary>
    public int Size { get; }

    #region Read methods

    public bool ReadBool(int index);
    public Byte ReadByte(int index);
    public SByte ReadSByte(int index);
    public char ReadChar(int index);
    public Int16 ReadInt16(int index);
    public UInt16 ReadUInt16(int index);
    public Int32 ReadInt32(int index);
    public UInt32 ReadUInt32(int index);
    public Int64 ReadInt64(int index);
    public UInt64 ReadUInt64(int index);
    public Double ReadDouble(int index);
    public Decimal ReadDecimal(int index);
    public Single ReadSingle(int index);
    public Guid ReadGuid(int index);
    public byte[] ReadBytes(int index, int length);
    public DateTime ReadDateTime(int index);
    public string ReadString(int index, int length, TextEncoding textEncoding = TextEncoding.UTF8);
    public object Read(DocumentType documentType, int index, int? length = null, TextEncoding textEncoding = TextEncoding.UTF8);

    #endregion

    #region Write methods

    public void Write(bool value, int index);
    public void Write(byte value, int index);
    public void Write(sbyte value, int index);
    public void Write(char value, int index);
    public void Write(Int16 value, int index);
    public void Write(UInt16 value, int index);
    public void Write(Int32 value, int index);
    public void Write(UInt32 value, int index);
    public void Write(Int64 value, int index);
    public void Write(UInt64 value, int index);
    public void Write(Double value, int index);
    public void Write(Single value, int index);
    public void Write(Decimal value, int index);
    public void Write(Guid value, int index);
    public void Write(byte[] value, int index);
    public void Write(DateTime value, int index);
    public void Write(string value, int index, TextEncoding textEncoding = TextEncoding.UTF8);
    public void Write(object value, int index, TextEncoding textEncoding = TextEncoding.UTF8);

    #endregion
}