using Dbarone.Net.Database.Thrift;

/// <summary>
/// Description for column metadata
/// </summary>
public sealed class ColumnMetaData
{
/** Type of this column **/
  1: required Type type

  /** Set of all encodings used for this column. The purpose is to validate
   * whether we can decode those pages. **/
  2: required list<Encoding> encodings

  /** Path in schema **/
  3: required list<string> path_in_schema

  /** Compression codec **/
  4: required CompressionCodec codec

  /** Number of values in this column **/
  5: required i64 num_values

  /** total byte size of all uncompressed pages in this column chunk (including the headers) **/
  6: required i64 total_uncompressed_size

  /** total byte size of all compressed, and potentially encrypted, pages
   *  in this column chunk (including the headers) **/
  7: required i64 total_compressed_size

  /** Optional key/value metadata **/
  8: optional list<KeyValue> key_value_metadata

  /** Byte offset from beginning of file to first data page **/
  9: required i64 data_page_offset

  /** Byte offset from beginning of file to root index page **/
  10: optional i64 index_page_offset

  /** Byte offset from the beginning of file to first (only) dictionary page **/
  11: optional i64 dictionary_page_offset

  /** optional statistics for this column chunk */
  12: optional Statistics statistics;

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