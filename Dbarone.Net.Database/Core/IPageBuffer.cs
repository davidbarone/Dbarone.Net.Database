namespace Dbarone.Net.Database;

/// <summary>
/// Describes an in-memory page buffer.
/// </summary>
public interface IPageBuffer
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
    /// Returns a byte array representation of the PageBuffer.
    /// </summary>
    /// <returns></returns>
    public byte[] ToArray();

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
    public string ReadString(int index, int length);

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
    public void Write(DateTime  value, int index);
    public void Write(string value, int index);

    #endregion

}