





public class ParquetThriftRowGroup
{
  List<columnch

}



public class ParquetThriftEncryptionAlgorithm
{

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