




public class ParquetThriftLogicalType
{

}

public class ParquetThriftRowGroup
{
  List<columnch

}

public class ParquetThriftKeyValue
{
  /// <summary>
  /// Field: 1
  /// </summary>
  public string Key { get; set; }

  /// <summary>
  /// Field: 2
  /// </summary>
  public string? Value { get; set; }
}

public class ParquetThriftColumnOrder
{

}

public class ParquetThriftEncryptionAlgorithm
{

}

public class ParquetThriftFileMetaData
{
  [ThriftFieldId(1)]
  public int Version { get; set; }

  [ThriftFieldId(2)]
  public List<ParquetThriftSchemaElement> Schema { get; set; }

  [ThriftFieldId(3)]
  public int NumRows { get; set; }

  [ThriftFieldId(4)]
  public List<ParquetThriftRowGroup> RowGroups { get; set; }

  [ThriftFieldId(5)]
  public List<ParquetThriftKeyValue>? KeyValueMetaData { get; set; }

  [ThriftFieldId(6)]
  public string? CreatedBy { get; set; }

  [ThriftFieldId(7)]
  public List<ParquetThriftColumnOrder>? ColumnOrders { get; set; }

  [ThriftFieldId(8)]
  public ParquetThriftEncryptionAlgorithm EncryptionAlgorithm { get; set; }

  [ThriftFieldId(9)]
  public byte[] FooterSigningKeyMetadata { get; set; }
}

public class ParquetThriftColumnChunk
{
  [ThriftFieldId(1)]
  public string? FilePath { get; set; }

  [ThriftFieldId(2)]
  public long FileOffset { get; set; }

  [ThriftFieldId(3)]
  public ParquetThriftColumnMetadata Metadata { get; set; }

  [ThriftFieldId(4)]
  public long OffsetIndexOffset { get; set; }

  [ThriftFieldId(5)]
  public long OffsetIndexLength { get; set; }

  [ThriftFieldId(6)]
  public long ColumnIndexOffset { get; set; }

  [ThriftFieldId(7)]
  public long ColumnIndexLength { get; set; }

  [ThriftFieldId(8)]
  public ParquetThriftColumnCryptoMetadata CryptoMetadata { get; set; }

  [ThriftFieldId(9)]
  public byte[] EncryptedColumnMetadata { get; set; }

}

public class ParquetThriftColumnMetadata
{

}

public class ParquetThriftColumnCryptoMetadata
{

}