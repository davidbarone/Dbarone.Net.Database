using Dbarone.Net.Document;
using System.Collections;

namespace Dbarone.Net.Database;

/// <summary>
/// Defines mapping operations between Table objects and sequences of POCO objects.
/// </summary>
public interface ITableMapper
{
    #region From Tables

    public IEnumerable MapTableToIEnumerable(Table table, Type toElementType);

    public IEnumerable<IDictionary<string, object>> MapTableToIEnumerableDictionary(Table table);

    public IEnumerable<T> MapTableToIEnumerable<T>(Table table);

    #endregion

    #region To Tables

    public Table MapIEnumerableToTable<T>(IEnumerable<T> data);

    public Table MapIEnumerableDictionaryToTable(IEnumerable<IDictionary<string, object>> data);

    public Table MapIEnumerableToTable(IEnumerable data);

    #endregion
}