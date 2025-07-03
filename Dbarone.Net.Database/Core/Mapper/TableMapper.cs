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
    public IEnumerable Map(Table table, Type toType)
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

    public IEnumerable<T> Map<T>(Table table)
    {
        throw new NotImplementedException();
    }

    public Table Map<T>(IEnumerable<T> data)
    {
        throw new NotImplementedException();
    }

    public Table Map(IEnumerable data)
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

    public IEnumerable<IDictionary<string, object>> Map(Table table)
    {
        throw new NotImplementedException();
    }

    public Table MapFromDictionary(IEnumerable<IDictionary<string, object>> data)
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
