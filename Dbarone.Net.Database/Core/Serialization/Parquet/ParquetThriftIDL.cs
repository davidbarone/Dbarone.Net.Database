
using System.Dynamic;

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
  /// <summary>
  /// Field: 1
  /// </summary>
  public ParquetThriftType? Type { get; set; }

  /// <summary>
  /// Field: 2
  /// </summary>
  public int? TypeLength { get; set; }

  /// <summary>
  /// Field: 3
  /// </summary>
  public ParquetThriftFieldRepetitionType RepetitionType { get; set; }

  /// <summary>
  /// Field: 4
  /// </summary>
  public string Name { get; set; }

  /// <summary>
  /// Field: 5
  /// </summary>
  public int? NumChildren { get; set; }

  /// <summary>
  /// Field: 8
  /// </summary>
  public int? Precision { get; set; }

  /// <summary>
  /// Field: 9
  /// </summary>
  public int? FieldId { get; set; }

  /// <summary>
  /// Field: 10
  /// </summary>
  public ParquetThriftLogicalType? LogicalType { get; set; }
}
public class ParquetThriftFileMetaData
{
  /// <summary>
  /// Field: 1
  /// </summary>
  public int Version { get; set; }

  /// <summary>
  /// Field: 2
  /// </summary>
  public List<ParquetThriftSchemaElement> Schema { get; set; }

  /// <summary>
  /// Field: 3
  /// </summary>
  public int NumRows { get; set; }

  /// <summary>
  /// Field: 4
  /// </summary>
  public List<ParquetThriftRowGroup> RowGroups { get; set; }

  /// <summary>
  /// Field: 5
  /// </summary>
  public List<ParquetThriftKeyValue>? KeyValueMetaData { get; set; }

  /// <summary>
  /// Field: 6
  /// </summary>
  public string? CreatedBy { get; set; }

  /// <summary>
  /// Field: 7
  /// </summary>
  public List<ParquetThriftColumnOrder>? ColumnOrders { get; set; }

  /// <summary>
  /// Field: 8
  /// </summary>
  public ParquetThriftEncryptionAlgorithm EncryptionAlgorithm { get; set; }

  /// <summary>
  /// Field: 9
  /// </summary>
  public byte[] FooterSigningKeyMetadata { get; set; }
}