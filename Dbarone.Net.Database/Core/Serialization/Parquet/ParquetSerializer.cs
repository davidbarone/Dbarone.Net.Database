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

        // Check the type of page
        if (ph.PageType == Dbarone.Net.Database.Parquet.PageType.DICTIONARY_PAGE)
        {
          var dict = GetDictionary(ph.DictionaryPageHeader!, chunk.Metadata!.Type, pageHeaderBuffer);
        }
        else if (ph.PageType == Dbarone.Net.Database.Parquet.PageType.DATA_PAGE)
        {
          List<TableRow> rows = new List<TableRow>();
          var raw = GetDataPage(chunk.Metadata.Type, ph.DataPageHeader, pageHeaderBuffer);
          foreach (var item in raw)
          {
            TableRow tr = new TableRow(columnName, item);
            rows.Add(tr);
          }
          model.Data = new Table(rows);
        }
      }
    }
    return model;
  }

  private PageHeader GetPageHeader(IBuffer buffer)
  {
    // Get the current

  }

  /// <summary>
  /// Gets a dictionary page.
  /// </summary>
  /// <param name="buffer">The parquet buffer.</param>
  /// <returns>Returns a dictionary page.</returns>
  private IList<object> GetDictionary(DictionaryPageHeader header, Dbarone.Net.Database.Parquet.Type type, IBuffer buffer)
  {
    if (header is null)
    {
      throw new Exception("Dictionary page header is null!");
    }

    // get the encoding
    var enc = header.Encoding;

    if (enc == Encoding.PLAIN_DICTIONARY)
    {
      List<object> dictionary = new List<object>();
      for (int i = 0; i < header.NumValues; i++)
      {
        // Get the type of dictionary entry:
        switch (type)
        {
          case Dbarone.Net.Database.Parquet.Type.INT32:
            // INT32 always stored in little-endian
            var bytesInt32 = buffer.ReadBytes(4);
            if (!BitConverter.IsLittleEndian)
            {
              // reverse bytes on big-endian systems (most x86 systems are little-endian)
              Array.Reverse(bytesInt32);
            }
            dictionary.Add(BitConverter.ToInt32(bytesInt32, 0));
            break;
          case Dbarone.Net.Database.Parquet.Type.INT64:
            // INT64 always stored in little-endian
            var bytesInt64 = buffer.ReadBytes(8);
            if (!BitConverter.IsLittleEndian)
            {
              // reverse bytes on big-endian systems (most x86 systems are little-endian)
              Array.Reverse(bytesInt64);
            }
            dictionary.Add(BitConverter.ToInt64(bytesInt64, 0));
            break;
          default:
            throw new Exception($"Unsupported dictionary type: {type}");
        }
      }
      return dictionary;
    }
    else
    {
      // only PLAIN encoding currently supported for dictionaries
      throw new Exception("Only PLAIN encoding currently supported for dictionaries.");
    }
  }

  private IEnumerable<object> GetDataPage(Dbarone.Net.Database.Parquet.Type type, DataPageHeader dataPageHeader, IBuffer buffer)
  {
    // Get the encoding in the page:
    switch (dataPageHeader.Encoding)
    {
      case Encoding.DELTA_BINARY_PACKED:
        // for int32 and int64
        DeltaBinaryPackedEncoder encoder = new DeltaBinaryPackedEncoder();
        var result = encoder.Decode(buffer);
        foreach (var item in result)
        {
          if (type == Dbarone.Net.Database.Parquet.Type.INT32)
          {
            yield return (int)item;
          }
          else if (type == Dbarone.Net.Database.Parquet.Type.INT64)
          {
            yield return item;
          }
          else
          {
            throw new Exception($"Invalid type: {type}");
          }
        }
        break;
      default:
        throw new Exception($"Encoding {dataPageHeader.Encoding} not supported.");
    }
  }
}