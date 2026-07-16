using Dbarone.Net.Database;
using Dbarone.Net.Database.Parquet;


/// <summary>
/// Parquet is an open source, column-oriented data file format designed for
/// efficient data storage and retrieval.
/// 
/// The Parquet format document can be found here: https://parquet.apache.org/
/// 
/// Parquet files use Parquet.Thrift:
/// (https://github.com/apache/parquet-format/blob/master/src/main/thrift/parquet.thrift)
/// To store metadata in Parquet files.
/// 
/// Parquet.Thrift is encoded using the Thrift Compact Protocol encoding:
/// https://github.com/apache/thrift/blob/master/doc/specs/thrift-compact-protocol.md
/// </summary>
public class ParquetSerializer
{
  public FileMetaData GetFileMetaData(IBuffer buffer, TextEncoding textEncoding = TextEncoding.UTF8)
  {
    ThriftCompactProtocolCodec codec = new ThriftCompactProtocolCodec();
    var fileMetaData = codec.Decode(buffer);
    return null;
  }

  public ParquetModel Read(byte[] bytes, TextEncoding textEncoding = TextEncoding.UTF8)
  {
    IBuffer buffer = new GenericBuffer(bytes);
    return Read(buffer, textEncoding);
  }


  /// <summary>
  /// Deserializes a buffer contains parquet-formatted data, into a table.
  /// </summary>
  /// <param name="buffer"></param>
  /// <param name="textEncoding"></param>
  /// <returns></returns>
  public ParquetModel Read(IBuffer buffer, TextEncoding textEncoding = TextEncoding.UTF8)
  {
    // Create return object
    ParquetModel model = new ParquetModel();

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
    var mdSer = new ThriftMetaDataSerializer();
    model.MetaData = mdSer.GetMetaData(metadataBuffer);

    return model;
  }
}