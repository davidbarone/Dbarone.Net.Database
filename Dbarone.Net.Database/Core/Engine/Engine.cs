namespace Dbarone.Net.Database;
using Dbarone.Net.Database.Mapper;
using System.Linq;
using Dbarone.Net.Assertions;
using Dbarone.Net.Extensions;

/// <summary>
/// The internal (private) database implementation.
/// </summary>
public class Engine : IEngine
{
    private string _filename;
    private Stream _stream;
    public BufferManager BufferManager { get; set; }

    #region Static members

    /// <summary>
    /// Creates an in-memory database, and writes the core system pages required for initial setup.
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IEngine Create(CreateDatabaseOptions? options = null)
    {
        if (options is null)
        {
            options = new CreateDatabaseOptions();
        }

        var engine = new Engine(options);
        return engine;
    }

    /// <summary>
    /// Creates a new disk-based database, and writes the core system pages required for initial setup.
    /// </summary>
    /// <param name="filename">The filename.</param>
    /// <returns></returns>
    public static IEngine Create(string filename, CreateDatabaseOptions options = null)
    {
        var engine = new Engine(filename, options);

        if (options is null)
        {
            options = new CreateDatabaseOptions();
        }

        // Create boot page (page #0)
        var bootPage = engine.BufferManager.Create(PageType.Boot);
        //BootData header = (BootData)bootPage.Header;
        //header.CreationTime = DateTime.Now;
        //header.TextEncoding = options.TextEncoding;
        //header.PageSize = options.PageSize;
        //header.OverflowThreshold = options.OverflowThreshold;

        return engine;
    }

    public static bool Exists(string filename)
    {
        return File.Exists(filename);
    }

    public static Engine Open(string filename, bool readOnly)
    {
        return new Engine(filename, readOnly: false);
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

    #region Constructor / destructor

    /// <summary>
    /// Creates a new in-memory database.
    /// </summary>
    /// <param name="options">The options to create the new database.</param>
    private Engine(CreateDatabaseOptions? options = null)
    {
        if (options is null)
        {
            options = new CreateDatabaseOptions();
        }

        //ISerializer serializer = new Serializer(options.PageSize, options.TextEncoding);
        //this.BufferManager = new MemoryBufferManager(options.PageSize, serializer);
    }

    /// <summary>
    /// Instantiates a new Engine object based on a disk file.
    /// </summary>
    /// <param name="filename">The filename (database).</param>
    /// <param name="canWrite">Set to true to allow writes.</param>
    public Engine(string filename, CreateDatabaseOptions options)
    {
        if (options is null)
        {
            throw new Exception("Must set create database options.");
        }

        this._filename = filename;
        var exists = File.Exists(this._filename);

        if (exists)
        {
            throw new Exception("File already exists");
        }

        this._stream = new FileStream(
            filename,
            FileMode.CreateNew,
            FileAccess.Write | FileAccess.Read,
            FileShare.None,
            options.PageSize);


        // Reconfigure the buffer manager with the actual page size + encoding for this database.
        //var serializer = new Serializer(options.PageSize, options.TextEncoding);
        //this.BufferManager = new DiskBufferManager(this._stream, options.PageSize, serializer);
    }

    /// <summary>
    /// Instantiates a new Engine object based on an existing disk file.
    /// </summary>
    /// <param name="filename">The filename (database).</param>
    /// <param name="canWrite">Set to true to allow writes.</param>
    public Engine(string filename, bool readOnly = false)
    {
        this._filename = filename;
        var exists = File.Exists(this._filename);

        if (!exists)
        {
            throw new Exception("Database file does not exist.");
        }

        // Open database intially with 512 page size, to read page 0.
        // Need to get the actual page size + text encoding information.
        this._stream = new FileStream(
            filename,
            FileMode.Open,
            readOnly ? FileAccess.Read : FileAccess.Write | FileAccess.Read,
            FileShare.None,
            512);

        //ISerializer serializer = new Serializer(512, TextEncoding.UTF8);
        //this.BufferManager = new DiskBufferManager(this._stream, 512, serializer);
        var boot = this.BufferManager.GetBootData();

        // Reconfigure the buffer manager with the actual page size + encoding for this database.
        this._stream = new FileStream(
            filename,
            FileMode.Open,
            readOnly ? FileAccess.Read : FileAccess.Write | FileAccess.Read,
            FileShare.None,
            boot.PageSize);
        //serializer = new Serializer(boot.PageSize, boot.TextEncoding);
        //this.BufferManager = new DiskBufferManager(this._stream, boot.PageSize, serializer);
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

    #region Metadata

    public DatabaseInfo Database()
    {/*
        var page0 = this.BufferManager.Get(0);
        var mapper = Mapper.ObjectMapper<BootData, DatabaseInfo>.Create();
        return mapper.MapOne(page0.BootData)!;
        */
        return null;
    }

    public IEnumerable<TableInfo> Tables()
    {
        /*
        var systemTablePage = this.GetPage<SystemTablePage>(1);
        var mapper = ObjectMapper<SystemTablePageData, TableInfo>.Create();
        var data = systemTablePage._data.Select(r => (r as SystemTablePageData)!);
        var mapped = mapper.MapMany(data!);
        return mapped;
        */
        return null;
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

    #endregion

    #region DQL

    public IEnumerable<IDictionary<string, object?>> ReadRaw(string tableName)
    {
        /*
        var table = Table(tableName);
        HeapTableManager<DictionaryPageData, DataPage> heap = new HeapTableManager<DictionaryPageData, DataPage>(this._bufferManager, this._serializer, table.ObjectId);
        return heap.Scan().Select(r => r.Row);
        */
        return null;
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
        /*
        var table = Table(tableName);
        var dict = new DictionaryPageData(row);
        var heap = new HeapTableManager<DictionaryPageData, DataPage>(this._bufferManager, this._serializer, table.ObjectId);
        return heap.AddRow(dict);
        */
        return 0;
    }

    public int BulkInsertRaw(string tableName, IEnumerable<IDictionary<string, object?>> rows)
    {
        /*
        if (!rows.Any())
        {
            return 0;
        }

        var table = Table(tableName);
        var heap = new HeapTableManager<DictionaryPageData, DataPage>(this._bufferManager, this._serializer, table.ObjectId);
        return heap.AddRows(rows.Select(r => new DictionaryPageData(r)));
        */
        return 0;
    }

    public int UpdateRaw(string tableName, Func<IDictionary<string, object?>, IDictionary<string, object?>> transform, Func<IDictionary<string, object?>, bool> predicate)
    {
        /*
        var table = Table(tableName);
        HeapTableManager<DictionaryPageData, DataPage> heap = new HeapTableManager<DictionaryPageData, DataPage>(this._bufferManager, this._serializer, table.ObjectId);
        return heap.UpdateRows(
            ((dictionaryPageData) => { return new DictionaryPageData(transform.Invoke(dictionaryPageData.Row)!); }),
            ((dictionaryPageData) => predicate.Invoke(dictionaryPageData.Row))
        );
        */
        return 0;
    }

    public int DeleteRaw(string tableName, Func<IDictionary<string, object?>, bool> predicate)
    {
        /*
        var table = Table(tableName);
        HeapTableManager<DictionaryPageData, DataPage> heap = new HeapTableManager<DictionaryPageData, DataPage>(this._bufferManager, this._serializer, table.ObjectId);
        return heap.DeleteRows((dictionaryPageData) => predicate.Invoke(dictionaryPageData.Row));
        */
        return 0;
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
        /*
        var table = Table(tableName);
        HeapTableManager<DictionaryPageData, DataPage> heap = new HeapTableManager<DictionaryPageData, DataPage>(this._bufferManager, this._serializer, table.ObjectId);
        return heap.UpdateRows(
            ((dictionaryPageData) => { return new DictionaryPageData(transform.Invoke(dictionaryPageData.Row.ToObject<T>()!).ToDictionary()!); }),
            ((dictionaryPageData) => predicate.Invoke(dictionaryPageData.Row.ToObject<T>()!))
        );
        */
        return 0;
    }

    public int Delete<T>(string tableName, Func<T, bool> predicate) where T : class
    {
        /*
        var table = Table(tableName);
        HeapTableManager<DictionaryPageData, DataPage> heap = new HeapTableManager<DictionaryPageData, DataPage>(this._bufferManager, this._serializer, table.ObjectId);
        return heap.DeleteRows((dictionaryPageData) => predicate.Invoke(dictionaryPageData.Row.ToObject<T>()!));
        */
        return 0;
    }

    #endregion

    /// <summary>
    /// Writes all dirty pages to disk.
    /// </summary>
    public void CheckPoint()
    {
        this.BufferManager.Save();
    }

    public TableInfo CreateTable(string tableName)
    {
        /*
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
        var columns = this._serializer.GetColumnsForType(typeof(T));

        foreach (var column in columns)
        {
            var obj = new SystemColumnPageData(parentObjectId, column.Name, column.DataType, column.IsNullable);
            var columnMeta = this._serializer.GetColumnsForType(systemColumnPage.PageDataType);
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
        */
        return null;
    }


    public IndexInfo CreateIndex(string tableName, string indexName, IList<string> columns, bool unique)
    {
        throw new Exception("xxx");
    }

    public void DropIndex(string indexName)
    {

    }

    #region Unsupported

    public TableInfo GetTableInfo(string tableName) { throw new NotSupportedException("Not supported."); }


    public bool BeginTransaction() { throw new NotSupportedException("Not supported."); }
    public bool CommitTransaction() { throw new NotSupportedException("Not supported."); }
    public bool RollbackTransaction() { throw new NotSupportedException("Not supported."); }

    #endregion    

    #region Debugging

    public string DebugPages()
    {
        return null;
    }

    public string DebugPage(int pageId)
    {
        return null;
    }

    #endregion

}
