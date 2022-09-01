namespace Dbarone.Net.Database;
public class Database : IDatabase
{
    private string _filename;
    private int _bufferSize = 8192;
    private IEngine _engine;

    public void CheckPoint() {
        _engine.CheckPoint();
    }

    /// <summary>
    /// Gets publicly available metadata about the database.
    /// </summary>
    /// <returns></returns>
    public DatabaseInfo GetDatabaseInfo()
    {
        var page0 = this._engine.GetPage<BootPage>(0);
        return new DatabaseInfo
        {
            Magic = page0.Headers().Magic,
            Version = page0.Headers().Version,
            CreationTime = page0.Headers().CreationTime,
            PageCount = page0.Headers().PageCount
        };
    }

    public Database(IEngine engine)
    {
        this._engine = engine;
    }

    public static Database Create(string filename)
    {
        var engine = Engine.Create(filename);
        return new Database(engine);
    }

    public static Database Open(string filename, bool canWrite)
    {
        return new Database(new Engine(filename, canWrite: true));
    }

    public static void Delete(string filename)
    {

        // open file and check valid database file.
        var database = Database.Open(filename, false);
        var info = database.GetDatabaseInfo();
        if (info.Magic != "Dbarone.Net.Database")
        {
            throw new Exception("File is not valid format.");
        }

        // If got here, good to delete
        File.Delete(filename);
    }

    #region DDL

    public TableInfo CreateTable<T>(string tableName){
        return null;
    }

    #endregion

    public void Dispose()
    {
        if (this._engine != null)
        {
            this._engine.Dispose();
        }
    }

    #region IDatabase

    public TableInfo GetTableInfo(string tableName) { throw new NotSupportedException("Not supported."); }
    public IEnumerable<ColumnInfo> GetColumnInfo(string tableName) { throw new NotSupportedException("Not supported."); }
    public IEnumerable<T> Read<T>(string tableName) { throw new NotSupportedException("Not supported."); }

    public bool BeginTransaction() { throw new NotSupportedException("Not supported."); }
    public bool CommitTransaction() { throw new NotSupportedException("Not supported."); }
    public bool RollbackTransaction() { throw new NotSupportedException("Not supported."); }

    public int insert<T>(string table, T data) { throw new NotSupportedException("Not supported."); }
    public int insert<T>(string table, IEnumerable<T> data) { throw new NotSupportedException("Not supported."); }
    public int update<T>(string table, T data) { throw new NotSupportedException("Not supported."); }
    public int update<T>(string table, IEnumerable<T> data) { throw new NotSupportedException("Not supported."); }
    public int upsert<T>(string table, T data) { throw new NotSupportedException("Not supported."); }
    public int upsert<T>(string table, IEnumerable<T> data) { throw new NotSupportedException("Not supported."); }
    public int delete<T>(string table, T data) { throw new NotSupportedException("Not supported."); }
    public int delete<T>(string table, IEnumerable<T> data) { throw new NotSupportedException("Not supported."); }

    #endregion
}
