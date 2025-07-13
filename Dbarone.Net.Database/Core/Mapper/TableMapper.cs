using Dbarone.Net.Assertions;
using System.Collections;
using Dbarone.Net.Extensions.Object;
using Dbarone.Net.Document;
using Dbarone.Net.Mapper;
using System.Data.SqlTypes;

namespace Dbarone.Net.Database;

/// <summary>
/// Base class for Serializer. A serializer perform serialize and deserialize functions to convert .NET objects to and from byte[] arrays.
/// </summary>
public class TableMapper : ITableMapper
{
    public IEnumerable MapTableToIEnumerable(Table table, Type toType)
    {
        // Build mapper
        var conf = new MapperConfiguration()
                    .SetAutoRegisterTypes(true)
                    .RegisterResolvers<TableRowMemberResolver>()
                    .RegisterResolvers<DictionaryMemberResolver>()
                    .RegisterOperator<TableMapperOperator>()
                    .RegisterOperator<TableRowMapperOperator>();

        var mapper = new ObjectMapper(conf);
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
        var conf = new MapperConfiguration()
                    .SetAutoRegisterTypes(true)
                    .RegisterResolvers<TableRowMemberResolver>()
                    .RegisterResolvers<DictionaryMemberResolver>()
                    .RegisterOperator<TableMapperOperator>()
                    .RegisterOperator<TableRowMapperOperator>();

        var mapper = new ObjectMapper(conf);
        return (Table)mapper.Map(typeof(Table), data)!;
    }

    public IEnumerable<IDictionary<string, object>> MapTableToIEnumerableDictionary(Table table)
    {
        // Build mapper
        var conf = new MapperConfiguration()
                    .SetAutoRegisterTypes(true)
                    .RegisterResolvers<TableResolver>()
                    .RegisterResolvers<TableRowMemberResolver>()
                    .RegisterResolvers<DictionaryMemberResolver>()
                    .RegisterOperator<TableMapperOperator>();
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
        var conf = new MapperConfiguration()
                    .SetAutoRegisterTypes(true)
                    .RegisterResolvers<TableResolver>()
                    .RegisterResolvers<TableRowMemberResolver>()
                    .RegisterResolvers<DictionaryMemberResolver>()
                    .RegisterOperator<TableMapperOperator>();
        //.RegisterOperator<TableRowMapperOperator>();

        var mapper = new ObjectMapper(conf);
        var op = mapper.GetMapperOperator<IEnumerable<IDictionary<string, object>>, Table>();
        var obj = op.Map(data);
        return (Table)obj!;
    }
}
