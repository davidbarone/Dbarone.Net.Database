namespace Dbarone.Net.Database;
using Dbarone.Net.Mapper;

/// <summary>
/// The internal (private) database implementation.
/// </summary>
public class Engine : IEngine
{
    private string _filename;
    private Stream _stream;
    private BufferManager _bufferManager;

    /// <summary>
    /// Writes all dirty pages to disk.
    /// </summary>
    public void CheckPoint()
    {
        this._bufferManager.SavePages();
    }

    public TableInfo CreateTable<T>(string tableName){
        var systemTablePage = this.GetPage<SystemTablePage>(1);
        SystemTablePageData row = new SystemTablePageData()
        {
            TableName = tableName,
            PageId = 0,
            IsSystemTable = false
        };
        systemTablePage.AddDataRow(row);
        return null;
    }

    public IEnumerable<TableInfo> Tables() {
        var systemTablePage = this.GetPage<SystemTablePage>(1);
        var mapper = ObjectMapper<SystemTablePageData, TableInfo>.Create();
        var data = systemTablePage.Data();
        var mapped = mapper.MapMany(data);
        return mapped;
    }

    public T GetPage<T>(int pageId) where T : Page
    {
        return this._bufferManager.GetPage<T>(pageId);
    }

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

    /// <summary>
    /// Creates a new database, and writes the core system pages required for functioning.
    /// </summary>
    /// <param name="filename">The filename.</param>
    /// <returns></returns>
    public static Engine Create(string filename)
    {
        var engine = new Engine(filename, canWrite: true);

        // Create boot page (page #0)
        var bootPage = engine.CreatePage<BootPage>();
        bootPage.Headers().CreationTime = DateTime.Now;

        // Create System table page (page #1)
        var systemTablePage = engine.CreatePage<SystemTablePage>();        

        // Create System column page (page #2)
        var systemColumnPage = engine.CreatePage<SystemColumnPage>();        

        return engine;
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

        // Update boot page PageCount
        var bootPage = this.GetPage<BootPage>(0);
        bootPage.Headers().PageCount++;

        // Update the pageId
        var page = this._bufferManager.GetPage<T>(pageId);
        page.Headers().PageId = pageId;

        return page;
    }

    public static Engine Open(string filename, bool canWrite)
    {
        return new Engine(filename, canWrite: true);
    }

    public static void Delete(string filename)
    {
        File.Delete(filename);
    }

    public void Dispose()
    {
        if (this._stream != null)
        {
            //this._stream.Close();
            this._stream.Dispose();
        }
    }
}
