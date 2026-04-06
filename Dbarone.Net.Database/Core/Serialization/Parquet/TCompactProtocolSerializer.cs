using Dbarone.Net.Database;
using Dbarone.Net.Extensions;

/// <summary>
/// Codes / decodes using the Thrift compact binary protocol.
/// - https://thrift.apache.org/static/files/thrift-20070401.pdf
/// - https://thrift.apache.org/
/// - https://simonkjohnston.life/thrift-specs/protocol-compact.html
/// - https://issues.apache.org/jira/browse/THRIFT-110
/// </summary>
public class TCompactProtocolSerializer
{

  /// <summary>
  /// Returns the low 4 bits of a byte.
  /// </summary>
  /// <param name="input">Input byte.</param>
  /// <returns>Returns low 4 bits.</returns>
  private int GetLowNibble(byte input)
  {
    return input & 0xF;
  }

  /// <summary>
  /// Returns the high 4 bits of a byte.
  /// </summary>
  /// <param name="input">Input byte.</param>
  /// <returns>Returns high 4 bits.</returns>
  private int GetHighNibble(byte input)
  {
    return (input & ~0xF) >> 4;
  }

  /// <summary>
  /// Reads the next field from a buffer.
  /// </summary>
  /// <param name="buffer">The input buffer.</param>
  /// <returns>Returns an object representing the field</returns>
  private (TCompactProtocolType FieldType, int FieldID, object? Value) ReadField(IBuffer buffer, int currentFieldID)
  {
    var (fieldType, fieldIDDelta) = ReadFieldHeader(buffer);
    var newfieldID = currentFieldID + fieldIDDelta;
    switch (fieldType)
    {
      case TCompactProtocolType.CT_I16:
        var zz = ReadZigZag(buffer);
        buffer.Position += zz.VarInt.Size;
        return (fieldType, newfieldID, zz.Decoded);
      case TCompactProtocolType.CT_I32:
        zz = ReadZigZag(buffer);
        buffer.Position += zz.VarInt.Size;
        return (fieldType, newfieldID, zz.Decoded);
      case TCompactProtocolType.CT_I64:
        zz = ReadZigZag(buffer);
        buffer.Position += zz.VarInt.Size;
        return (fieldType, newfieldID, zz.Decoded);
    }
  }

  private Dictionary<int, object?> ReadStruct(IBuffer buffer)
  {
    Dictionary<int, object?> dict = new Dictionary<int, object?>();
    var field = ReadField(buffer, 0);
    while (field.FieldType != TCompactProtocolType.CT_STOP)
    {
      dict.Add(field.FieldID, field.Value);
    }
    return dict;
  }

  private VarInt ReadVarInt(IBuffer buffer)
  {
    // create copy of buffer starting at varint.
    var slice = buffer.Slice(buffer.Position, buffer.Length - buffer.Position);
    var vi = new VarInt(slice);
    return vi;
  }

  private ZigZag ReadZigZag(IBuffer buffer)
  {
    var vi = ReadVarInt(buffer);
    return new ZigZag(vi);
  }

  /// <summary>
  /// Gets a field header.
  /// </summary>
  /// <param name="buffer">The input buffer to read.</param>
  /// <returns>Returns the field type and field ID delta.</returns>
  private (TCompactProtocolType FieldType, int FieldIDDelta) ReadFieldHeader(IBuffer buffer)
  {
    var b = buffer.ReadBytes(1)[0];
    var low = GetLowNibble(b);
    var high = GetHighNibble(b);
    return (
      FieldType: (TCompactProtocolType)low,
      FieldIDDelta: high
      );
  }

  public object Decode(IBuffer buffer)
  {

  }

  public object DecodeI16(IBuffer buffer)
}