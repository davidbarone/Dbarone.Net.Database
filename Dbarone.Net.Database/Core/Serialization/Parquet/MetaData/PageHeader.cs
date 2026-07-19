namespace Dbarone.Net.Database.Parquet;

[ParquetThriftMetaData()]
public sealed class PageHeader
{
  /// <summary>
  /// the type of the page: indicates which of the *_header fields is set
  /// </summary>
  [FieldId(1)]
  public PageType PageType { get; set; }

  /// <summary>
  /// Uncompressed page size in bytes (not including this header)
  /// </summary>
  [FieldId(2)]
  public int UncompressedPageSize { get; set; }

  /// <summary>
  /// Compressed (and potentially encrypted) page size in bytes, not including this header
  /// </summary>
  [FieldId(3)]
  public int CompressedPageSize { get; set; }


  /// <summary>
  /// The 32-bit CRC checksum for the page, to be calculated as follows:
  /// 
  /// - The standard CRC32 algorithm is used(with polynomial 0x04C11DB7,
  /// the same as in e.g.GZIP).
  /// - All page types can have a CRC(v1 and v2 data pages, dictionary pages,
  /// etc.).
  /// - The CRC is computed on the serialization binary representation of the page
  /// (as written to disk), excluding the page header.For example, for v1
  /// data pages, the CRC is computed on the concatenation of repetition levels,
  /// definition levels and column values (optionally compressed, optionally
  /// encrypted).
  /// - The CRC computation therefore takes place after any compression
  /// and encryption steps, if any.
  /// 
  /// If enabled, this allows for disabling checksumming in HDFS if only a few
  /// pages need to be read.
  /// </summary>
  [FieldId(4)]
  public int? Crc { get; set; }

  DataPageHeader? DataPageHeader { get; set; }

  IndexPageHeader? IndexPageHeader { get; set; }

  DictionaryPageHeader? DictionaryPageHeader { get; set; }

  DataPageHeaderV2? DataPageHeaderV2 { get; set; }
}

/// <summary>
/// Data page header.
/// </summary>
public sealed class DataPageHeader
{
  /// <summary>
  /// Number of values, including NULLs, in this data page.
  /// 
  /// If an OffsetIndex is present, a page must begin at a row
  /// boundary (repetition_level = 0). Otherwise, pages may begin
  /// within a row(repetition_level > 0).
  /// </summary>
  [FieldId(1)]
  public int NumValues { get; set; }

  /// <summary>
  /// Encoding used for this data page.
  /// </summary>
  [FieldId(2)]
  public Encoding Encoding { get; set; }

  /// <summary>
  /// Encoding used for definition levels.
  /// </summary>
  [FieldId(3)]
  public Encoding DefinitionLevelEncoding { get; set; }

  /// <summary>
  /// Encoding used for repetition levels.
  /// </summary>
  [FieldId(4)]
  public Encoding RepetitionLevelEncoding { get; set; }

  /// <summary>
  /// Optional statistics for the data in this page.
  /// </summary>
  [FieldId(5)]
  public Statistics? Statistics { get; set; }
}
