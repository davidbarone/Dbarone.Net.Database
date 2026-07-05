namespace Dbarone.Net.Database.Thrift;

public sealed class RowGroup
{
  /// <summary>
  /// Metadata for each column chunk in this row group.
  /// This list must have the same order as the SchemaElement list in FileMetaData.
  /// </summary>
  public List<ColumnChunk> Columns { get; set; } = default!;

  /// <summary>
  /// Total byte size of all the uncompressed column data in this row group
  /// </summary>
  public long TotalByteSize { get; set; }

  /// <summary>
  /// Number of rows in this row group
  /// </summary>
  public long NumRows { get; set; }

  /// <summary>
  /// If set, specifies a sort ordering of the rows in this RowGroup.
  /// The sorting columns can be a subset of all the columns.
  /// </summary>
  public List<SortingColumn>? SortingColumns { get; set; }

  /// <summary>
  /// Byte offset from beginning of file to first page (data or dictionary)
  /// in this row group
  /// </summary>
  public long? FileOffset { get; set; }

  /// <summary>
  /// Total byte size of all compressed (and potentially encrypted) column data
  /// in this row group
  /// </summary>
  public long? TotalCompressedSize { get; set; }

  /// <summary>
  /// Row group ordinal in the file
  /// </summary>
  public short? Ordinal { get; set; }
}