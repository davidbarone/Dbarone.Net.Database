namespace Dbarone.Net.Database;
using Dbarone.Net.Mapper;
using System.Linq;

/// <summary>
/// The internal (private) database implementation.
/// </summary>
public class Engine : IEngine
{
    private string _filename;
    private Stream _stream;
    private BufferManager _bufferManager;

    #region Constructor / destructor

    /// <summary>
    /// Instantiates a new Engine object.
    /// </summary>
    /// <param name="filename">The filename (database).</param>
    /// <param name="canWrite">Set to true to allow writes.</param>
    public Engine(string filename, bool canWrite)
    {
        this._filename = filename;
        var exists = File.Exists(this._filename);
        this._stream = new FileStream(
            filename,
            exists ? FileMode.Open : FileMode.OpenOrCreate,
            canWrite ? FileAccess.Write | FileAccess.Read : FileAccess.Read,
            FileShare.None,
            (int)Global.PageSize);

        this._bufferManager = new BufferManager(new DiskService(this._stream));
    }

    public void Dispose()
    {
        if (this._stream != null)
        {
            //this._stream.Close();
            this._stream.Dispose();
        }
    }

    #endregion

    #region Static members

    /// <summary>
    /// Creates a new database, and writes the core system pages required for functioning.
    /// </summary>
    /// <param name="filename">The filename.</param>
    /// <returns></returns>
    public static IEngine Create(string filename)
    {
        var engine = new Engine(filename, canWrite: true);

        // Create boot page (page #0)
        var bootPage = engine.CreatePage<BootPage>();
        bootPage.Headers().CreationTime = DateTime.Now;

        // Create System table page (page #1)
        var systemTablePage = engine.CreatePage<SystemTablePage>();

        return engine;
    }

    public static Engine Open(string filename, bool canWrite)
    {
        return new Engine(filename, canWrite: true);
    }

    public static void Delete(string filename)
    {
        // open file and check valid database file.
        var db = Open(filename, false);
        var info = db.Database();
        if (info.Magic == "Dbarone.Net.Database")
        {
            // If got here, good to delete
            File.Delete(filename);
            return;
        }
        throw new Exception("File is not valid format.");
    }

    #endregion

    #region Metadata

    public DatabaseInfo Database()
    {
        var page0 = this.GetPage<BootPage>(0);
        return new DatabaseInfo
        {
            Magic = page0.Headers().Magic,
            Version = page0.Headers().Version,
            CreationTime = page0.Headers().CreationTime,
            PageCount = page0.Headers().PageCount
        };
    }

    public IEnumerable<TableInfo> Tables()
    {
        var systemTablePage = this.GetPage<SystemTablePage>(1);
        var mapper = ObjectMapper<SystemTablePageData, TableInfo>.Create();
        var data = systemTablePage.Data();
        var mapped = mapper.MapMany(data);
        return mapped;
    }

    public TableInfo Table(string tableName) {
        var table = Tables().FirstOrDefault(t => t.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase));
        if (table == null)
        {
            throw new Exception($"Invalid table name: [{tableName}].");
        }
        return table;
    }

    public IEnumerable<ColumnInfo> Columns(string tableName)
    {
        var table = Table(tableName);
        var systemColumnPage = this.GetPage<SystemColumnPage>(table.ColumnPageId);
        var mapper = ObjectMapper<SystemColumnPageData, ColumnInfo>.Create();
        var mapped = mapper.MapMany(systemColumnPage.Data());
        return mapped;
    }

    #endregion

    #region DML

    public int insert(string tableName, IDictionary<string, object?> row){
        var table = Table(tableName);
        var data = GetPage<DataPage>(table.PageId);
        data.AddDataRow(new DictionaryPageData(row));
        return 0;
    }

    #endregion

    /// <summary>
    /// Writes all dirty pages to disk.
    /// </summary>
    public void CheckPoint()
    {
        this._bufferManager.SavePages();
    }

    public TableInfo CreateTable<T>(string tableName)
    {
        var systemTablePage = this.GetPage<SystemTablePage>(1);
        var systemColumnPage = this.CreatePage<SystemColumnPage>();
        var dataPage = this.CreatePage<DataPage>();
        SystemTablePageData row = new SystemTablePageData()
        {
            TableName = tableName,
            PageId = dataPage.Headers().PageId,
            IsSystemTable = false,
            ColumnPageId = systemColumnPage.Headers().PageId,
        };
        systemTablePage.AddDataRow(row);
        var columns = Serializer.GetColumnsForType(typeof(T));
        foreach (var column in columns)
        {
            systemColumnPage.AddDataRow(new SystemColumnPageData(column.Name, column.DataType, column.IsNullable));
        }
        return null;
    }

    public TableInfo CreateTable(string tableName, IEnumerable<ColumnInfo> columns)
    {
        var systemTablePage = this.GetPage<SystemTablePage>(1);
        var systemColumnPage = this.CreatePage<SystemColumnPage>();
        var dataPage = this.CreatePage<DataPage>();
        SystemTablePageData row = new SystemTablePageData()
        {
            TableName = tableName,
            PageId = dataPage.Headers().PageId,
            IsSystemTable = false,
            ColumnPageId = systemColumnPage.Headers().PageId
        };
        systemTablePage.AddDataRow(row);
        foreach (var column in columns)
        {
            systemColumnPage.AddDataRow(new SystemColumnPageData(column.Name, column.DataType, column.IsNullable));
        }
        return null;
    }

    public T GetPage<T>(int pageId) where T : Page
    {
        return this._bufferManager.GetPage<T>(pageId);
    }

    /// <summary>
    /// Creates a new page. Also updates the PageCount header on the boot page.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private T CreatePage<T>() where T : Page
    {
        int pageId = 0;
        if (typeof(T) == typeof(BootPage))
        {
            pageId = this._bufferManager.CreatePage(PageType.Boot);
        }
        else if (typeof(T) == typeof(SystemTablePage))
        {
            pageId = this._bufferManager.CreatePage(PageType.SystemTable);
        }
        else if (typeof(T) == typeof(SystemColumnPage))
        {
            pageId = this._bufferManager.CreatePage(PageType.SystemColumn);
        }
        else if (typeof(T) == typeof(DataPage))
        {
            pageId = this._bufferManager.CreatePage(PageType.Data);
        }

        // Update boot page PageCount
        var bootPage = this.GetPage<BootPage>(0);
        bootPage.Headers().PageCount++;

        // Update the pageId
        var page = this._bufferManager.GetPage<T>(pageId);
        page.Headers().PageId = pageId;

        return page;
    }


    #region Unsupported

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
