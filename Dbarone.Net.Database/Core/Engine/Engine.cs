namespace Dbarone.Net.Database;
using Dbarone.Net.Mapper;
using System.Linq;
using Dbarone.Net.Extensions.Object;
using Dbarone.Net.Assertions;

/// <summary>
/// The internal (private) database implementation.
/// </summary>
public class Engine : IEngine
{
    private string _filename;
    private Stream _stream;
    private BufferManager _bufferManager;

    HeapTableManager<SystemTablePageData, SystemTablePage>? _systemTablesHeap = null;
    private HeapTableManager<SystemTablePageData, SystemTablePage> SystemTablesHeap
    {
        get
        {
            if (_systemTablesHeap == null)
            {
                _systemTablesHeap = new HeapTableManager<SystemTablePageData, SystemTablePage>(this._bufferManager, null);
            }
            return _systemTablesHeap;
        }
    }

    /// <summary>
    /// Returns a reference to the boot page.
    /// </summary>
    /// <returns></returns>
    private BootPage GetBootPage()
    {
        return _bufferManager.GetPage<BootPage>(0);
    }

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
    public static IEngine Create(string filename, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        var engine = new Engine(filename, canWrite: true);

        // Create boot page (page #0)
        var bootPage = engine.CreatePage<BootPage>();
        bootPage.Headers().CreationTime = DateTime.Now;
        bootPage.Headers().TextEncoding = textEncoding;

        // Create System table page (page #1)
        var systemTablePage = engine.CreatePage<SystemTablePage>();

        return engine;
    }

    public static bool Exists(string filename)
    {
        return File.Exists(filename);
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
        var mapper = Mapper.ObjectMapper<IBootPageHeader, DatabaseInfo>.Create();
        return mapper.MapOne(page0.Headers())!;
    }

    public IEnumerable<TableInfo> Tables()
    {
        var systemTablePage = this.GetPage<SystemTablePage>(1);
        var mapper = ObjectMapper<SystemTablePageData, TableInfo>.Create();
        var data = systemTablePage._data.Select(r => (r as SystemTablePageData)!);
        var mapped = mapper.MapMany(data!);
        return mapped;
    }

    public TableInfo Table(string tableName)
    {
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
        var heap = new HeapTableManager<SystemColumnPageData, SystemColumnPage>(this._bufferManager, table.ObjectId);
        var mapper = ObjectMapper<SystemColumnPageData, ColumnInfo>.Create();
        var mapped = mapper.MapMany(heap.Scan());
        return mapped;
    }

    #endregion

    #region DQL

    public IEnumerable<IDictionary<string, object?>> ReadRaw(string tableName)
    {
        var table = Table(tableName);
        HeapTableManager<DictionaryPageData, DataPage> heap = new HeapTableManager<DictionaryPageData, DataPage>(this._bufferManager, table.ObjectId);
        return heap.Scan().Select(r => r.Row);
    }

    public IEnumerable<T?> Read<T>(string tableName) where T : class
    {
        foreach (var item in ReadRaw(tableName))
        {
            yield return item.ToObject<T>();
        }
    }

    #endregion

    #region DML

    public int InsertRaw(string tableName, IDictionary<string, object?> row)
    {
        var table = Table(tableName);
        var dict = new DictionaryPageData(row);
        var heap = new HeapTableManager<DictionaryPageData, DataPage>(this._bufferManager, table.ObjectId);
        return heap.AddRow(dict);
    }

    public int BulkInsertRaw(string tableName, IEnumerable<IDictionary<string, object?>> rows)
    {
        if (!rows.Any())
        {
            return 0;
        }

        var table = Table(tableName);
        var heap = new HeapTableManager<DictionaryPageData, DataPage>(this._bufferManager, table.ObjectId);
        return heap.AddRows(rows.Select(r => new DictionaryPageData(r)));
    }

    public int UpdateRaw(string tableName, Func<IDictionary<string, object?>, IDictionary<string, object?>> transform, Func<IDictionary<string, object?>, bool> predicate)
    {
        var table = Table(tableName);
        HeapTableManager<DictionaryPageData, DataPage> heap = new HeapTableManager<DictionaryPageData, DataPage>(this._bufferManager, table.ObjectId);
        return heap.UpdateRows(
            ((dictionaryPageData) => { return new DictionaryPageData(transform.Invoke(dictionaryPageData.Row)!); }),
            ((dictionaryPageData) => predicate.Invoke(dictionaryPageData.Row))
        );
    }

    public int DeleteRaw(string tableName, Func<IDictionary<string, object?>, bool> predicate)
    {
        var table = Table(tableName);
        HeapTableManager<DictionaryPageData, DataPage> heap = new HeapTableManager<DictionaryPageData, DataPage>(this._bufferManager, table.ObjectId);
        return heap.DeleteRows((dictionaryPageData) => predicate.Invoke(dictionaryPageData.Row));
    }

    public int Insert<T>(string table, T row)
    {
        Assert.NotNull(row);

        var dataDict = row as IDictionary<string, object?>;
        if (dataDict == null)
        {
            dataDict = row.ToDictionary();
        }
        if (dataDict != null)
        {
            return InsertRaw(table, dataDict);
        }
        else
        {
            throw new Exception("Should not get here.");
        }
    }

    public int BulkInsert<T>(string tableName, IEnumerable<T> rows)
    {
        var table = Table(tableName);

        if (!rows.Any())
        {
            return 0;
        }

        var _rows = rows.Select(r =>
        {
            var dict = r as IDictionary<string, object?>;
            if (dict == null)
            {
                dict = r.ToDictionary();
            }
            return dict;
        });
        return BulkInsertRaw(tableName, _rows!);
    }

    public int Update<T>(string tableName, Func<T, T> transform, Func<T, bool> predicate) where T : class
    {
        var table = Table(tableName);
        HeapTableManager<DictionaryPageData, DataPage> heap = new HeapTableManager<DictionaryPageData, DataPage>(this._bufferManager, table.ObjectId);
        return heap.UpdateRows(
            ((dictionaryPageData) => { return new DictionaryPageData(transform.Invoke(dictionaryPageData.Row.ToObject<T>()!).ToDictionary()!); }),
            ((dictionaryPageData) => predicate.Invoke(dictionaryPageData.Row.ToObject<T>()!))
        );
    }

    public int Delete<T>(string tableName, Func<T, bool> predicate) where T : class
    {
        var table = Table(tableName);
        HeapTableManager<DictionaryPageData, DataPage> heap = new HeapTableManager<DictionaryPageData, DataPage>(this._bufferManager, table.ObjectId);
        return heap.DeleteRows((dictionaryPageData) => predicate.Invoke(dictionaryPageData.Row.ToObject<T>()!));
    }

    #endregion

    /// <summary>
    /// Writes all dirty pages to disk.
    /// </summary>
    public void CheckPoint()
    {
        this._bufferManager.SaveDirtyPages();
    }

    public TableInfo CreateTable<T>(string tableName)
    {
        // Gets the unique object id for the new table.
        var parentObjectId = GetBootPage().GetNextObjectId();

        // Create table
        this.SystemTablesHeap.AddRow(new SystemTablePageData()
        {
            TableName = tableName,
            ObjectId = parentObjectId,
            DataPageId = 0,
            IsSystemTable = false,
            ColumnPageId = 0,
        });

        // Create columns
        var systemColumnPage = this.CreatePage<SystemColumnPage>();
        var columns = Serializer.GetColumnsForType(typeof(T));

        foreach (var column in columns)
        {
            var obj = new SystemColumnPageData(parentObjectId, column.Name, column.DataType, column.IsNullable);
            var columnMeta = Serializer.GetColumnsForType(systemColumnPage.PageDataType);
            var buffer = this._bufferManager.SerialiseRow(obj, RowStatus.None, columnMeta);
            systemColumnPage.AddDataRow(obj, buffer, false);
        }

        // Create data page
        var dataPage = this.CreatePage<DataPage>(parentObjectId);

        // Update the table metadata
        this.SystemTablesHeap.UpdateRows(r => new SystemTablePageData
        {
            ObjectId = parentObjectId,
            TableName = r.TableName,
            DataPageId = dataPage.Headers().PageId,
            IsSystemTable = r.IsSystemTable,
            ColumnPageId = systemColumnPage.Headers().PageId,
        }, r => r.ObjectId == parentObjectId);

        // return
        return Table(tableName);
    }

    public TableInfo CreateTable(string tableName, IEnumerable<ColumnInfo> columns)
    {
        // Gets the unique object id for the new table.
        var parentObjectId = GetBootPage().GetNextObjectId();

        // Create table
        this.SystemTablesHeap.AddRow(new SystemTablePageData()
        {
            TableName = tableName,
            ObjectId = parentObjectId,
            DataPageId = 0,
            IsSystemTable = false,
            ColumnPageId = 0
        });

        // Create columns
        var systemColumnPage = this.CreatePage<SystemColumnPage>();
        foreach (var column in columns)
        {
            var obj = new SystemColumnPageData(parentObjectId, column.Name, column.DataType, column.IsNullable);
            var columnMeta = Serializer.GetColumnsForType(systemColumnPage.PageDataType);
            var buffer = this._bufferManager.SerialiseRow(obj, RowStatus.None, columnMeta);
            systemColumnPage.AddDataRow(obj, buffer, false);
        }

        // Create data page
        var dataPage = this.CreatePage<DataPage>(parentObjectId);

        // Update the table metadata
        this.SystemTablesHeap.UpdateRows(r => new SystemTablePageData
        {
            ObjectId = parentObjectId,
            TableName = r.TableName,
            DataPageId = dataPage.Headers().PageId,
            IsSystemTable = r.IsSystemTable,
            ColumnPageId = systemColumnPage.Headers().PageId
        }, r => r.ObjectId == parentObjectId);

        // return
        return Table(tableName);
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
    private T CreatePage<T>(int? parentObjectId = null) where T : Page
    {
        var page = this._bufferManager.CreatePage<T>(parentObjectId);
        return page;
    }


    #region Unsupported

    public TableInfo GetTableInfo(string tableName) { throw new NotSupportedException("Not supported."); }
    public IEnumerable<ColumnInfo> GetColumnInfo(string tableName) { throw new NotSupportedException("Not supported."); }


    public bool BeginTransaction() { throw new NotSupportedException("Not supported."); }
    public bool CommitTransaction() { throw new NotSupportedException("Not supported."); }
    public bool RollbackTransaction() { throw new NotSupportedException("Not supported."); }

    #endregion    

    #region Debugging

    public string DebugPages()
    {
        return this._bufferManager.DebugPages();
    }

    public string DebugPage(int pageId)
    {
        return this._bufferManager.DebugPage(pageId);
    }

    #endregion

}
