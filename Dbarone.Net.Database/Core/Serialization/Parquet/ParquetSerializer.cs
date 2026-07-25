using Dbarone.Net.Database;
using Dbarone.Net.Database.Parquet;
using Dbarone.Net.Extensions;


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

    // Having got the metadata, we can now read the actual data
    // Order is: RowGroup -> ColumnChunk -> PageHeader -> DataPage
    // 1 parquet file can only have 1 column schema - all rows must have same colums + types

    // To store the results
    List<Dictionary<string, object?>> results = new List<Dictionary<string, object?>>();

    // Get the schema
    // Note that schema[0] is 'root'.
    var schema = model.MetaData.Schema;

    // Loop through each row group
    // row groups are unioned at the end
    foreach (var rowGroup in model.MetaData.RowGroups)
    {
      // loop through each column chunk in the columns.
      // each column chunk has same number of rows - the rows in the row group
      var numRows = rowGroup.NumRows;
      for (int i = 1; i < schema.Count; i++)  // ignore the 'root' schema element.
      {
        var columnName = schema[i].Name;  // column name
        var chunk = rowGroup.Columns[i - 1];
        // each column chunk in a row group is divided into pages.
        // get start and length of 1st page header for chunk
        var start = chunk.FileOffset;
        int size = (int)rowGroup.TotalByteSize;
        buffer.Position = start;
        var pageHeaderBytes = buffer.ReadBytes(size);
        GenericBuffer pageHeaderBuffer = new GenericBuffer(pageHeaderBytes);
        var ph = mdSer.GetPageHeader(pageHeaderBuffer);


        if (ph.DataPageHeader is not null)
        {
          var table = new Table();
          List<TableRow> rows = new List<TableRow>();
          var raw = GetDataPage(ph.DataPageHeader, buffer);
          foreach (var item in raw)
          {
            TableRow tr = new TableRow(columnName, item);
            rows.Add(tr);
          }
          model.Data = table;
        }
      }
    }
    return model;
  }

  private IEnumerable<object> GetDataPage(DataPageHeader dataPageHeader, IBuffer buffer)
  {
    throw new NotSupportedException();
  }
}