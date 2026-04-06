using System.Reflection.Metadata.Ecma335;
using Dbarone.Net.Database;
using Dbarone.Net.Extensions;

/// <summary>
/// Codes / decodes using the Thrift compact binary protocol.
/// - https://thrift.apache.org/static/files/thrift-20070401.pdf
/// - https://thrift.apache.org/
/// - https://simonkjohnston.life/thrift-specs/protocol-compact.html
/// - https://issues.apache.org/jira/browse/THRIFT-110
/// </summary>
public class TCompactProtocolDecoder
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
    if (fieldType == TCompactProtocolType.CT_STOP)
    {
      return (fieldType, currentFieldID, null);
    }
    else
    {
      var newfieldID = currentFieldID + fieldIDDelta;
      var value = ReadValue(buffer, fieldType);
      return (fieldType, newfieldID, value);
    }
  }

  public Dictionary<int, object?> ReadStruct(IBuffer buffer)
  {
    Dictionary<int, object?> dict = new Dictionary<int, object?>();
    var field = ReadField(buffer, 0);
    while (field.FieldType != TCompactProtocolType.CT_STOP)
    {
      dict.Add(field.FieldID, field.Value);
      field = ReadField(buffer, field.FieldID);
    }
    return dict;
  }

  private object ReadValue(IBuffer buffer, TCompactProtocolType type)
  {
    switch (type)
    {
      case TCompactProtocolType.CT_BOOLEAN_TRUE:
        return true;
      case TCompactProtocolType.CT_BOOLEAN_FALSE:
        return false;
      case TCompactProtocolType.CT_I16:
        var zz = ReadZigZag(buffer);
        return zz.Decoded;
      case TCompactProtocolType.CT_I32:
        zz = ReadZigZag(buffer);
        return zz.Decoded;
      case TCompactProtocolType.CT_I64:
        zz = ReadZigZag(buffer);
        return zz.Decoded;
      case TCompactProtocolType.CT_LIST:
        var list = ReadList(buffer);
        return list;
      case TCompactProtocolType.CT_STRUCT:
        var st = ReadStruct(buffer);
        return st;
      case TCompactProtocolType.CT_BINARY:
        // for byte[] and string types
        var length = ReadVarInt(buffer);
        var bytes = buffer.ReadBytes(length);
        return bytes;
      case TCompactProtocolType.CT_BYTE:
        var b = buffer.ReadBytes(1)[0];
        return b;
      case TCompactProtocolType.CT_STOP:
        return null;
      default:
        throw new Exception("whoops");
    }
  }

  public List<object> ReadList(IBuffer buffer)
  {
    List<object> arr = new List<object>();
    var listHeader = ReadListHeader(buffer);
    for (ulong i = 0; i < listHeader.Size; i++)
    {
      arr.Add(ReadValue(buffer, listHeader.ElementType));
    }
    return arr;
  }

  /// <summary>
  /// Reads a VarInt from a buffer. The buffer position is advanced by the size of the VarInt.
  /// </summary>
  /// <param name="buffer">The input buffer.</param>
  /// <returns>Returns a VarInt.</returns>
  private VarInt ReadVarInt(IBuffer buffer)
  {
    // create copy of buffer starting at varint.
    var slice = buffer.Slice(buffer.Position, buffer.Length - buffer.Position);
    var vi = new VarInt(slice);
    buffer.Position += vi.Size;
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

  /// <summary>
  /// Within a LIST, read the list header first.
  /// - high 4 bits: size (if < 15), otherwise, 0xF, then read varint for size
  /// - low 4 bits: element type
  /// </summary>
  /// <param name="buffer">The input buffer.</param>
  /// <returns>Returns list header</returns>
  private (TCompactProtocolType ElementType, ulong Size) ReadListHeader(IBuffer buffer)
  {
    var b = buffer.ReadBytes(1)[0];
    var low = GetLowNibble(b);
    var high = GetHighNibble(b);
    ulong size = (ulong)high;
    if (size == 15)
    {
      // read next VarInt to get size
      var vi = this.ReadVarInt(buffer);
      size = vi.Value;
    }
    return (
      ElementType: (TCompactProtocolType)low,
      Size: size
    );
  }

  public object Decode(IBuffer buffer)
  {
    return null;
  }

  public object DecodeI16(IBuffer buffer)
  {
    return null;
  }
}