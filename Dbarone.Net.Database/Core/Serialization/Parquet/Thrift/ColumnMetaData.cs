using Dbarone.Net.Database.Thrift;

/// <summary>
/// Description for column metadata
/// </summary>
public sealed class ColumnMetaData
{
  /// <summary>
  /// Type of this column
  /// </summary>
  [FieldId(1)]
  public Dbarone.Net.Database.Thrift.Type Type { get; set; }

  /// <summary>
  /// Set of all encodings used for this column. The purpose is to validate
  /// whether we can decode those pages. **/
  /// </summary>
  [FieldId(2)]
  public List<Encoding> Encodings { get; set; } = default!;

  /// <summary>
  /// Path in schema
  /// </summary>
  [FieldId(3)]
  public List<string> PathInSchema { get; set; } = default!;

  /// <summary>
  /// Compression codec
  /// </summary>
  [FieldId(4)]
  public CompressionCodec Codec { get; set; }

  /// <summary>
  /// Number of values in this column
  /// </summary>
  [FieldId(5)]
  public long NumValues { get; set; }

  /// <summary>
  /// total byte size of all uncompressed pages in this column chunk (including the headers)
  /// </summary>
  [FieldId(6)]
  public long TotalUncompressedSize { get; set; }

  /// <summary>
  /// total byte size of all compressed, and potentially encrypted, pages
  /// in this column chunk(including the headers)
  /// </summary>
  [FieldId(7)]
  public long TotalCompressedSize { get; set; }

  /// <summary>
  /// Optional key/value metadata
  /// </summary>
  [FieldId(8)]
  public List<KeyValue>? KeyValueMetaData { get; set; }

  /// <summary>
  /// Byte offset from beginning of file to first data page
  /// </summary>
  [FieldId(9)]
  public long DataPageOffset { get; set; }

  /// <summary>
  /// Byte offset from beginning of file to root index page
  /// </summary>
  [FieldId(10)]
  public long? IndexPageOffset { get; set; }

  /// <summary>
  /// Byte offset from the beginning of file to first (only) dictionary page
  /// </summary>
  [FieldId(11)]
  public long? DictionaryPageOffset { get; set; }

  /// <summary>
  /// optional statistics for this column chunk
  /// </summary>
  [FieldId(12)]
  public Statistics? Statistics { get; set; }

  /** Set of all encodings used for pages in this column chunk.
   * This information can be used to determine if all data pages are
   * dictionary encoded for example **/
  13: optional list<PageEncodingStats> encoding_stats;

  /** Byte offset from beginning of file to Bloom filter data. **/
  14: optional i64 bloom_filter_offset;

  /** Size of Bloom filter data including the serialized header, in bytes.
   * Added in 2.10 so readers may not read this field from old files and
   * it can be obtained after the BloomFilterHeader has been deserialized.
   * Writers should write this field so readers can read the bloom filter
   * in a single I/O.
   */
  15: optional i32 bloom_filter_length;

  /**
   * Optional statistics to help estimate total memory when converted to in-memory
   * representations. The histograms contained in these statistics can
   * also be useful in some cases for more fine-grained nullability/list length
   * filter pushdown.
   */
  16: optional SizeStatistics size_statistics;

  /** Optional statistics specific for Geometry and Geography logical types */
  17: optional GeospatialStatistics geospatial_statistics;
}