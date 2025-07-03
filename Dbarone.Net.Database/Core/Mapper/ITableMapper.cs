using Dbarone.Net.Document;
using System.Collections;

namespace Dbarone.Net.Database;

/// <summary>
/// Defines mapping operations between Table objects and sequences of POCO objects.
/// </summary>
public interface ITableMapper
{
    public IEnumerable Map(Table table, Type toElementType);

    public IEnumerable<IDictionary<string, object>> Map(Table table);

    public IEnumerable<T> Map<T>(Table table);


    public Table Map<T>(IEnumerable<T> data);

    public Table MapFromDictionary(IEnumerable<IDictionary<string, object>> data);

    public Table Map(IEnumerable data);
}