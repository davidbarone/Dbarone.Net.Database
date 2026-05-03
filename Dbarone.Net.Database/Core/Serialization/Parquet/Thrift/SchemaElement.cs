/// This file defines the Parquet Thrift interface (Thrift IDL)
/// https://github.com/apache/parquet-format/blob/master/src/main/thrift/parquet.thrift
namespace Dbarone.Net.Database.Thrift;

public class SchemaElement
{
  [ThriftFieldId(1)]
  public Type? Type { get; set; }

  [ThriftFieldId(2)]
  public int? TypeLength { get; set; }

  [ThriftFieldId(3)]
  public RepetitionType? RepetitionType { get; set; }

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
