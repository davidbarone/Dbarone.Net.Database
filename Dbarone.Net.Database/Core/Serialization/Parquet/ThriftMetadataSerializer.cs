using System.ComponentModel;
using System.Reflection;
using Dbarone.Net.Database;
using Dbarone.Net.Database.Parquet;
using MappingRulesAlias = System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.Dictionary<int, System.Reflection.PropertyInfo>>;

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
  private ThriftCompactProtocolCodec serializer = new ThriftCompactProtocolCodec();

  private bool IsParquetThriftMetaDataType(System.Type type)
  {
    if (type.GetCustomAttribute<ParquetThriftMetaDataAttribute>(inherit: false) != null)
    {
      return true;
    }
    else
    {
      return false;
    }
  }

  private IEnumerable<System.Type> GetMetaDataTypes()
  {
    foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
    {
      if (IsParquetThriftMetaDataType(type))
      {
        yield return type;
      }
    }
  }

  private MappingRulesAlias GetMappingRules()
  {
    var mappingRules = new MappingRulesAlias();

    foreach (var type in GetMetaDataTypes())
    {
      Dictionary<int, PropertyInfo> map = new Dictionary<int, PropertyInfo>();

      // Get all public properties
      foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
      {
        // Check if the property has a FieldId attribute
        var attr = property.GetCustomAttribute<FieldIdAttribute>();
        if (attr != null)
        {
          map.Add(attr.FieldId, property);
        }
      }

      // Add to rules
      mappingRules[type] = map;
    }
    return mappingRules;
  }


  private T MapDict<T>(Dictionary<int, object?> dict, MappingRulesAlias mappingRules)
  {
    var type = typeof(T);
    return (T)MapDict(type, dict, mappingRules);
  }

  private object MapDict(System.Type targetType, Dictionary<int, object?> dict, MappingRulesAlias mappingRules)
  {
    // Get rules for the target type
    if (!mappingRules.ContainsKey(targetType))
    {
      throw new Exception($"Type {targetType} cannot be used for mapping to Parquet Thrift Metadata.");
    }
    var rules = mappingRules[targetType];

    // Create empty object to be target of mapping
    var obj = Activator.CreateInstance(targetType);
    if (obj is null)
    {
      throw new Exception("whoops");
    }

    foreach (var item in dict.Keys)
    {
      if (!rules.ContainsKey(item))
      {
        throw new Exception($"The mapping fules for type: {targetType} do not contain a mapping for field id: {item}.");
      }

      var pi = rules[item];

      switch (pi.GetType())
      {
        case System.Type boolType when boolType == typeof(bool):
        case System.Type byteType when byteType == typeof(byte):
        case System.Type shortType when shortType == typeof(short):
        case System.Type intType when intType == typeof(int):
        case System.Type longType when longType == typeof(long):
          pi.SetValue(obj, dict[item]);
          break;
        case System.Type parquetThriftMetadataType when IsParquetThriftMetaDataType(parquetThriftMetadataType):
          pi.SetValue(obj, MapDict(parquetThriftMetadataType, (System.Collections.Generic.Dictionary<int, object?>)dict[item], mappingRules);
          break;
        default:
          throw new Exception("whoops");
      }
    }
    return obj;
  }

  public FileMetaData GetMetaData(IBuffer buffer)
  {
    // Deserialise to dict
    var dict = serializer.Decode(buffer);

    // Get mapping rules
    var mappingRules = GetMappingRules();

    // Map dict to Thrift object
    var metadata = MapDict<FileMetaData>(dict, mappingRules);

    return metadata;
  }
}