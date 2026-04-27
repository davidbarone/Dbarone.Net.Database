/// This file defines the Parquet Thrift interface (Thrift IDL)
/// https://github.com/apache/parquet-format/blob/master/src/main/thrift/parquet.thrift
namespace Dbarone.Net.Database;

public enum ParquetThriftType
{
  BOOLEAN = 0,
  INT32 = 1,
  INT64 = 2,
  INT96 = 3,  // deprecated, new Parquet writers should not write data in INT96
  FLOAT = 4,
  DOUBLE = 5,
  BYTE_ARRAY = 6,
  FIXED_LEN_BYTE_ARRAY = 7
}

public enum ParquetThriftFieldRepetitionType
{
  /** This field is required (can not be null) and each row has exactly 1 value. */
  REQUIRED = 0,

  /** The field is optional (can be null) and each row has 0 or 1 values. */
  OPTIONAL = 1,

  /** The field is repeated and can contain 0 or more values */
  REPEATED = 2,
}

public class ParquetThriftLogicalType
{

}

public class ParquetThriftRowGroup
{

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

public class ParquetThriftSchemaElement
{
  [ThriftFieldId(1)]
  public ParquetThriftType? Type { get; set; }

  [ThriftFieldId(2)]
  public int? TypeLength { get; set; }

  [ThriftFieldId(3)]
  public ParquetThriftFieldRepetitionType RepetitionType { get; set; }

  [ThriftFieldId(4)]
  public string Name { get; set; }

  [ThriftFieldId(5)]
  public int? NumChildren { get; set; }

  [ThriftFieldId(8)]
  public int? Precision { get; set; }

  [ThriftFieldId(9)]
  public int? FieldId { get; set; }

  [ThriftFieldId(10)]
  public ParquetThriftLogicalType? LogicalType { get; set; }
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