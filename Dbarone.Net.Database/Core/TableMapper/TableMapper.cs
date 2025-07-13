using Dbarone.Net.Assertions;
using System.Collections;
using Dbarone.Net.Document;
using Dbarone.Net.Database.Mapper;
using System.Data.SqlTypes;

namespace Dbarone.Net.Database;

/// <summary>
/// Base class for Serializer. A serializer perform serialize and deserialize functions to convert .NET objects to and from byte[] arrays.
/// </summary>
public class TableMapper : ITableMapper
{
    private MapperConfiguration BaseConfiguration => new MapperConfiguration()
        .SetAutoRegisterTypes(true)
        // need to register all base operators
        .RegisterOperators(
            typeof(ConverterMapperOperator),
            typeof(EnumSourceValueMapperOperator),
            typeof(EnumTargetValueMapperOperator),
            typeof(EnumSourceStringMapperOperator),
            typeof(EnumTargetStringMapperOperator),
            typeof(NullableSourceMapperOperator),
            typeof(AssignableMapperOperator),
            typeof(ObjectSourceMapperOperator),
            typeof(ConvertibleMapperOperator),
            typeof(ImplicitOperatorMapperOperator),
            typeof(EnumerableMapperOperator),
            typeof(MemberwiseMapperDeferBuildOperator),
            typeof(MemberwiseMapperOperator)
        )
        // custom operators + resolvers
        .RegisterOperator<TableMapperOperator>()
        .RegisterResolvers<TableResolver>()
        .RegisterOperator<TableRowMapperOperator>()
        .RegisterResolvers<TableRowMemberResolver>();

    public IEnumerable MapTableToIEnumerable(Table table, Type toType)
    {
        // Build mapper
        var mapper = new ObjectMapper(BaseConfiguration);
        return (IEnumerable)mapper.Map(toType, table)!;
    }

    public IEnumerable<T> MapTableToIEnumerable<T>(Table table)
    {
        throw new NotImplementedException();
    }

    public Table MapIEnumerableToTable<T>(IEnumerable<T> data)
    {
        throw new NotImplementedException();
    }

    public Table MapIEnumerableToTable(IEnumerable data)
    {
        // Build mapper
        var mapper = new ObjectMapper(BaseConfiguration);
        return (Table)mapper.Map(typeof(Table), data)!;
    }

    public IEnumerable<IDictionary<string, object>> MapTableToIEnumerableDictionary(Table table)
    {
        // Build mapper
        var conf = BaseConfiguration;
        //.RegisterOperator<TableRowMapperOperator>();

        // Converter to map TableCell to Object for dictionary values.
        conf.RegisterConverter<TableCell, object>((tc) => tc.RawValue!);

        var mapper = new ObjectMapper(conf);
        var op = mapper.GetMapperOperator<Table, IEnumerable<Dictionary<string, object>>>();
        var obj = op.Map(table);
        return (IEnumerable<IDictionary<string, object>>)obj!;
    }

    public Table MapIEnumerableDictionaryToTable(IEnumerable<IDictionary<string, object>> data)
    {
        // Build mapper
        var mapper = new ObjectMapper(BaseConfiguration);
        var op = mapper.GetMapperOperator<IEnumerable<IDictionary<string, object>>, Table>();
        var obj = op.Map(data);
        return (Table)obj!;
    }
}
