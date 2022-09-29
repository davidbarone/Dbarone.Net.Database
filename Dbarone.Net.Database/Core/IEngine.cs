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

    #endregion

    #region Queries

    IEnumerable<T> Read<T>(string tableName);

    #endregion

    #region Transactions

    bool BeginTransaction();
    bool CommitTransaction();
    bool RollbackTransaction();
    void CheckPoint();

    #endregion

    #region DML

    int Insert<T>(string table, T row);
    
    /// <summary>
    /// Inserts a row into a table using a dictionary of values.
    /// </summary>
    /// <param name="tableName">The name of the table to insert the row into.</param>
    /// <param name="row">The row data to insert.</param>
    /// <returns></returns>
    int InsertRaw(string tableName, IDictionary<string, object?> row);
    int Insert<T>(string table, IEnumerable<T> data);
    int update<T>(string table, T data);
    int update<T>(string table, IEnumerable<T> data);
    int upsert<T>(string table, T data);
    int upsert<T>(string table, IEnumerable<T> data);
    int delete<T>(string table, T data);
    int delete<T>(string table, IEnumerable<T> data);

    #endregion

    /// <summary>
    /// Gets a page.
    /// </summary>
    /// <param name="pageId"></param>
    /// <returns></returns>
    T GetPage<T>(int pageId) where T : Page;

}