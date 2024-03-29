namespace Dbarone.Net.Database;

/// <summary>
/// Represents the internal functioning of the database. Not accessible to end-users.
/// </summary>
public interface IEngine : IDisposable
{
    #region Metadata

    /// <summary>
    /// Gets the database information for the current database.
    /// </summary>
    /// <returns></returns>
    DatabaseInfo Database();

    /// <summary>
    /// Gets table information for a single table.
    /// </summary>
    /// <param name="tableName">The table name.</param>
    /// <returns>returns table information</returns>
    TableInfo Table(string tableName);

    /// <summary>
    /// Gets table information for all tables in the database.
    /// </summary>
    /// <returns>A collection of table information.</returns>
    IEnumerable<TableInfo> Tables();

    /// <summary>
    /// Returns the column information for a table.
    /// </summary>
    /// <param name="tableName">The table name.</param>
    /// <returns>Returns the column information for the table.</returns>
    IEnumerable<ColumnInfo> Columns(string tableName);

    #endregion

    #region DDL

    TableInfo CreateTable<T>(string tableName);
    TableInfo CreateTable(string tableName, IEnumerable<ColumnInfo> columns);

    IndexInfo CreateIndex(string tableName, string indexName, IList<string> columns, bool unique);
    void DropIndex(string indexName);

    #endregion

    #region DQL

    IEnumerable<T?> Read<T>(string tableName) where T : class;

    IEnumerable<IDictionary<string, object?>> ReadRaw(string tableName);

    #endregion

    #region TCL

    bool BeginTransaction();
    bool CommitTransaction();
    bool RollbackTransaction();
    void CheckPoint();

    #endregion

    #region DML

    int InsertRaw(string tableName, IDictionary<string, object?> row);
    int BulkInsertRaw(string tableName, IEnumerable<IDictionary<string, object?>> rows);
    int UpdateRaw(string tableName, Func<IDictionary<string, object?>, IDictionary<string, object?>> transformation, Func<IDictionary<string, object?>, bool> predicate);
    int DeleteRaw(string tableName, Func<IDictionary<string, object?>, bool> predicate);

    int Insert<T>(string tableName, T row);
    int BulkInsert<T>(string tableName, IEnumerable<T> rows);
    int Update<T>(string tableName, Func<T, T> transformation, Func<T, bool> predicate) where T : class;
    int Delete<T>(string tableName, Func<T, bool> predicate) where T : class;

    #endregion

    /// <summary>
    /// Gets a page.
    /// </summary>
    /// <param name="pageId"></param>
    /// <returns></returns>
    T GetPage<T>(int pageId) where T : Page;

    #region Debugging

    string DebugPages();

    string DebugPage(int pageId);

    #endregion
}