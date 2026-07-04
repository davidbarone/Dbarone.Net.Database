/// This file defines the Parquet Thrift interface (Thrift IDL)
/// https://github.com/apache/parquet-format/blob/master/src/main/thrift/parquet.thrift
namespace Dbarone.Net.Database.Thrift;

/// <summary>
/// Represents an element inside a schema definition.
///  - if it is a group (inner node) then type is undefined and num_children is defined
///  - if it is a primitive type (leaf) then type is defined and num_children is undefined
/// the nodes are listed in depth first traversal order.
/// </summary>
public class SchemaElement
{
  [FieldId(1)]
  public Type? Type { get; set; }

  [FieldId(2)]
  public int? TypeLength { get; set; }

  [FieldId(3)]
  public RepetitionType? RepetitionType { get; set; }

  [FieldId(4)]
  public string Name { get; set; } = default!;

  [FieldId(5)]
  public int? NumChildren { get; set; }

  [Deprecated()]
  [FieldId(6)]
  public ConvertedType? ConvertedType { get; set; }

  [Deprecated()]
  [FieldId(7)]
  public int? Scale { get; set; }

  [Deprecated()]
  [FieldId(8)]
  public int? Precision { get; set; }

  [FieldId(9)]
  public int? FieldId { get; set; }

  [FieldId(10)]
  public LogicalType? LogicalType { get; set; }
}
