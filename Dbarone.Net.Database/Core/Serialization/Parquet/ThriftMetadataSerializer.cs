using System.Collections;
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

  static bool IsGenericListType(System.Type type)
  {
    if (type == null)
      return false;

    return type.IsGenericType &&
           type.GetGenericTypeDefinition() == typeof(List<>);
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

  private System.Type GetGenericListElementType(System.Type type)
  {
    if (type.IsGenericType && type.IsAssignableToGenericType(typeof(IList<>)))
    {
      return type.GetGenericArguments()[0];
    }
    else
    {
      throw new Exception($"Type {type.Name} is not a generic list type.");
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

  private IList ToGenericList(System.Type elementType, List<object> sourceArray)
  {
    // Get the cast method
    var castMethod = typeof(IEnumerable).GetExtensionMethods().First(m => m.Name == "Cast");
    // Get the cast method for the element type parameter, and invoke;
    var results = castMethod.MakeGenericMethod(elementType).Invoke(null, new object[] { sourceArray });
    if (results == null)
    {
      throw new Exception("Null return value for ToGenericList().");
    }
    var toListMethod = typeof(IEnumerable<>).GetExtensionMethods().First(m => m.Name == "ToList");
    var returnValue = toListMethod.MakeGenericMethod(elementType).Invoke(null, new object[] { results }) as IList;
    if (returnValue == null)
    {
      throw new Exception("Null return value for ToGenericList().");
    }
    return returnValue;
  }

  private object MapOne(System.Type targetType, object? value, MappingRulesAlias mappingRules)
  {
    // Maps 1 value
    switch (targetType)
    {
      case System.Type boolType when boolType == typeof(bool):
        return Convert.ToBoolean(value);
      case System.Type byteType when byteType == typeof(byte):
        return Convert.ToByte(value);
      case System.Type shortType when shortType == typeof(short):
        return Convert.ToInt16(value);
      case System.Type intType when intType == typeof(int):
        return Convert.ToInt32(value);
      case System.Type longType when longType == typeof(long):
        return Convert.ToInt64(value);
      case System.Type listType when IsGenericListType(listType):
        var elementType = GetGenericListElementType(listType);
        return MapList(elementType, (IList)value!, mappingRules);
      case System.Type parquetThriftMetadataType when IsParquetThriftMetaDataType(parquetThriftMetadataType):
        return MapDict(parquetThriftMetadataType, (System.Collections.Generic.Dictionary<int, object?>)value!, mappingRules);
      default:
        throw new Exception($"Mapping does not exist for targetType of: {targetType}.");
    }
  }

  private IList MapList(System.Type targetElementType, IEnumerable sourceArray, MappingRulesAlias mappingRules)
  {
    var list = new List<object>();

    foreach (var item in sourceArray)
    {
      var mapped = MapOne(targetElementType, item, mappingRules);
      list.Add(mapped);
    }
    return ToGenericList(targetElementType, list);
  }

  private T MapDict<T>(Dictionary<int, object?> dict, MappingRulesAlias mappingRules)
  {
    var type = typeof(T);
    return (T)MapDict(type, dict, mappingRules);
  }

  private object MapDict(System.Type targetType, Dictionary<int, object?> sourceDict, MappingRulesAlias mappingRules)
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

    foreach (var item in sourceDict.Keys)
    {
      if (!rules.ContainsKey(item))
      {
        throw new Exception($"The mapping fules for type: {targetType} do not contain a mapping for field id: {item}.");
      }

      var pi = rules[item];
      var mapped = MapOne(pi.PropertyType, sourceDict[item], mappingRules);
      pi.SetValue(obj, mapped);
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