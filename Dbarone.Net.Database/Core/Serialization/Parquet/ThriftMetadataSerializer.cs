using System.ComponentModel;
using System.Formats.Tar;
using System.Reflection;
using Dbarone.Net.Database;
using Dbarone.Net.Database.Parquet;
using Dbarone.Net.Extensions;
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

  public bool IsEnumerableType(System.Type type)
  {
    if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type) || type.IsArray)
    {
      return true;
    }

    System.Type[] interfaces = type.GetInterfaces();
    foreach (System.Type type2 in interfaces)
    {
      if (type2.GetTypeInfo().IsGenericType && type2.GetGenericTypeDefinition() == typeof(IEnumerable<>))
      {
        return true;
      }
    }

    return false;
  }

  public bool IsDictionaryType(System.Type type)
  {
    if (!type.GetTypeInfo().IsGenericType || !type.GetGenericTypeDefinition().Equals(typeof(IDictionary<,>)))
    {
      return type.GetInterfaces().Any((System.Type x) => x == typeof(System.Collections.IDictionary) || (x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition().Equals(typeof(IDictionary<,>))));
    }

    return true;
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

      switch (pi.PropertyType)
      {
        case System.Type boolType when boolType == typeof(bool):
          pi.SetValue(obj, Convert.ToBoolean(dict[item]));
          break;
        case System.Type byteType when byteType == typeof(byte):
          pi.SetValue(obj, Convert.ToByte(dict[item]));
          break;
        case System.Type shortType when shortType == typeof(short):
          pi.SetValue(obj, Convert.ToInt16(dict[item]));
          break;
        case System.Type intType when intType == typeof(int):
          pi.SetValue(obj, Convert.ToInt32(dict[item]));
          break;
        case System.Type longType when longType == typeof(long):
          pi.SetValue(obj, Convert.ToInt64(dict[item]));
          break;
        case System.Type arrayType when IsEnumerableType(arrayType) && !IsDictionaryType(arrayType):
          break;
        case System.Type parquetThriftMetadataType when IsParquetThriftMetaDataType(parquetThriftMetadataType):
          pi.SetValue(obj, MapDict(parquetThriftMetadataType, (System.Collections.Generic.Dictionary<int, object?>)dict[item], mappingRules));
          break;
        default:
          throw new Exception($"Mapping does not exist for property: {pi.Name} of type {pi.PropertyType}.");
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