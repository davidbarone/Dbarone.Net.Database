namespace Dbarone.Net.Database;

/// <summary>
/// Describes operations that can be performed on a buffer.
/// </summary>
public interface IBuffer
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
    public long Length { get; }

    /// <summary>
    /// Gets / sets the current position in the stream for next read or write.
    /// </summary>
    public long Position { get; set; }

    #region Read methods

    public bool ReadBool();
    public Byte ReadByte();
    public SByte ReadSByte();
    public char ReadChar();
    public Int16 ReadInt16();
    public UInt16 ReadUInt16();
    public Int32 ReadInt32();
    public UInt32 ReadUInt32();
    public Int64 ReadInt64();
    public UInt64 ReadUInt64();
    public Double ReadDouble();
    public Decimal ReadDecimal();
    public Single ReadSingle();
    public Guid ReadGuid();
    public byte[] ReadBytes(int length);
    public DateTime ReadDateTime();
    public string ReadString(int length, TextEncoding textEncoding = TextEncoding.UTF8);
    public object Read(DocumentType dataType, int? length = null, TextEncoding textEncoding = TextEncoding.UTF8);

    #endregion

    #region Write methods

    public int Write(bool value);
    public int Write(byte value);
    public int Write(sbyte value);
    public int Write(char value);
    public int Write(Int16 value);
    public int Write(UInt16 value);
    public int Write(Int32 value);
    public int Write(UInt32 value);
    public int Write(Int64 value);
    public int Write(UInt64 value);
    public int Write(Double value);
    public int Write(Single value);
    public int Write(Decimal value);
    public int Write(Guid value);
    public int Write(byte[] value);
    public int Write(DateTime value);
    public int Write(string value, TextEncoding textEncoding = TextEncoding.UTF8);
    public int Write(object value, TextEncoding textEncoding = TextEncoding.UTF8);

    #endregion

}