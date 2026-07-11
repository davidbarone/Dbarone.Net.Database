using System.ComponentModel;
using System.Reflection;
using Dbarone.Net.Database;
using Dbarone.Net.Database.Parquet;

/// <summary>
/// Parquet files store metadata using the Apache Thrift
/// Interface Definition Language (IDL).
/// 
/// The Parquet Thrift format can be found here:
/// https://github.com/apache/parquet-format/blob/master/src/main/thrift/parquet.thrift
/// 
/// Parquet Thrift uses TCompactProtocol for serialisation. This is an
/// efficient binary serialisation protocol:
/// https://github.com/apache/thrift/blob/master/doc/specs/thrift-compact-protocol.md
/// </summary>
public class ThriftMetaDataSerializer
{
  private Dictionary<System.Type, Dictionary<int, PropertyInfo>> MappingRules = new Dictionary<Dbarone.Net.Database.Parquet.Type, Dictionary<int, PropertyInfo>>();

  private IEnumerable<System.Type> GetMetaDataTypes()
  {
    foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
    {
      if (type.GetCustomAttribute<ParquetThriftMetaDataAttribute>(inherit: false) != null)
      {
        yield return type;
      }
    }
  }

  private void Init()
  {
    foreach (var type in GetMetaDataTypes())
    {
      // Get all public properties
      foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
      {
        // Check if the property has a FieldId attribute

      }
    }

  }

  private TCompactProtocolDecoder serializer = new TCompactProtocolDecoder();

  private T MapDict<T>(Dictionary<int, object?> dict)
  {

  }

  private object MapDict(System.Type targetType, Dictionary<int, object?> dict)
  {

  }

  public FileMetaData GetMetaData(IBuffer buffer)
  {
    // Deserialise to dict
    var dict = serializer.ReadStruct(buffer);

    // Map dict to Thrift object
    var ps

  }
}