namespace Dbarone.Net.Database;

/// <summary>
/// Represents the public interface to the database. The user's connection to the database.
/// </summary>
public interface IDatabase : IDisposable {

    #region Database

    #endregion

    #region Metadata

    DatabaseInfo GetDatabaseInfo();
    TableInfo GetTableInfo(string tableName);
    IEnumerable<ColumnInfo> GetColumnInfo(string tableName);

    #endregion

    #region Queries

    IEnumerable<T> Read<T>(string tableName);

    #endregion

    #region Transactions

    bool BeginTransaction();
    bool CommitTransaction();
    bool RollbackTransaction();

    #endregion

    #region DML

    int insert<T>(string table, T data);
    int insert<T>(string table, IEnumerable<T> data);
    int update<T>(string table, T data);
    int update<T>(string table, IEnumerable<T> data);
    int upsert<T>(string table, T data);
    int upsert<T>(string table, IEnumerable<T> data);
    int delete<T>(string table, T data);
    int delete<T>(string table, IEnumerable<T> data);

    #endregion
}