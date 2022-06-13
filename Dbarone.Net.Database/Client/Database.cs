namespace Dbarone.Net.Database;
public class Database : IDatabase
{
    private string _filename;
    private Stream _stream;
    private int _bufferSize = 8192;
    private IEngine _engine;

    /// <summary>
    /// Gets publicly available metadata about the database.
    /// </summary>
    /// <returns></returns>
    public DatabaseInfo GetDatabaseInfo(){
        var page0 = this._engine.GetPage<BootPage>(0);
        return new DatabaseInfo
        {
            Magic = page0.Magic,
            Version = page0.Version,
            CreationTime = page0.CreationTime
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

    public static void Delete(string filename){
        
        // open file and check valid database file.
        var database = Database.Open(filename, false);
        var info = database.GetDatabaseInfo();
        if (info.Magic != "Dbarone.Net.Database"){
            throw new Exception("File is not valid format.");
        }
        
        // If got here, good to delete
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
