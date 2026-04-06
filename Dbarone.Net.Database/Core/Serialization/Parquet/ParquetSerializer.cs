using Dbarone.Net.Database;

public class ParquetSerializer
{
  /// <summary>
  /// Deserializes a buffer contains parquet-formatted data, into a table.
  /// </summary>
  /// <param name="buffer"></param>
  /// <param name="textEncoding"></param>
  /// <returns></returns>
  public Table Deserialize(IBuffer buffer, TextEncoding textEncoding = TextEncoding.UTF8)
  {
    // Magic header
    buffer.Position = 0;
    var magicHeader = buffer.ReadString(4);
    if (!magicHeader.Equals("PAR1"))
    {
      throw new Exception("Invalid magic header");
    }

    // Magic footer
    buffer.Position = buffer.Length - 4;
    var magicFooter = buffer.ReadString(4);
    if (!magicFooter.Equals("PAR1"))
    {
      throw new Exception("Invalid magic footer");
    }

    // Get file metadata length - 4 bytes immediately prior to magic footer - 4 bytes in little-endian format
    buffer.Position = buffer.Length - 4 - 4;
    var fileMetadataLengthBytes = buffer.ReadBytes(4);
    int fileMetadataLength = BitConverter.ToInt32(fileMetadataLengthBytes, 0);

    // Get metadata
    // Encoded in Apache Thrift compact/binary protocol (FileMetaData struct)
    // https://thrift.apache.org/
    buffer.Position = buffer.Length - 4 - 4 - fileMetadataLength;
    var fileMetadataBytes = buffer.ReadBytes(fileMetadataLength);
    GenericBuffer metadataBuffer = new GenericBuffer(fileMetadataBytes);

    var field_id = 0;

    // Field #1: version
    // structure: field header
    // - high 4 bits: delta field id
    // - low 4 bits: type 
    // - typically Type: I32, Delta fieldid: 1
    var versionByte = (int)metadataBuffer.ReadBytes(1)[0];
    var versionLow = versionByte & 0xF;
    var versionHigh = (versionByte & ~0xF) >> 4;
    field_id += versionHigh;

    // Next byte[s] are version in ZigZag encoding
    var versionVarInt = new VarInt(metadataBuffer.Slice(metadataBuffer.Position, metadataBuffer.Length - metadataBuffer.Position));
    var versionZz = new ZigZag(versionVarInt);
    var version = versionZz.Decoded;
    metadataBuffer.Position = metadataBuffer.Position + versionVarInt.Size;

    // Field #2: List<SchemaElement>
    var schemaListByte = (int)metadataBuffer.ReadBytes(1)[0];
    var schemaListLow = schemaListByte & 0xF;
    var schemaListHigh = (schemaListByte & ~0xF) >> 4;
    field_id += schemaListHigh;

    // Within a LIST, read the list header
    // - high 4 bits: element type
    // - low 4 bits: size (if < 15), otherwise, 0xF, then read varint for size
    var listHeaderByte = (int)metadataBuffer.ReadBytes(1)[0];
    var listHeaderLow = listHeaderByte & 0xF;
    var listHeaderHigh = (listHeaderByte & ~0xF) >> 4;

    if (listHeaderLow == 0xF)
    {
      // read varint to get size
      var viSize = new VarInt(metadataBuffer.Slice(metadataBuffer.Position, metadataBuffer.Length - metadataBuffer.Position));
      var zz = new ZigZag(viSize);
    }


    return new Table();
  }
}