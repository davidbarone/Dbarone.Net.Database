namespace Dbarone.Net.Database;

/// <summary>
/// The internal (private) database implementation.
/// </summary>
public class Engine : IEngine
{
    private string _filename;
    private Stream _stream;
    private int _bufferSize = 8192;
    private DiskService _diskService;
    private BufferManager _bufferManager;

    public T GetPage<T>(int pageId) where T:Page {
        return this._bufferManager.GetPage<T>(pageId);
    }

    /// <summary>
    /// Instantiates a new Engine object
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
            canWrite ? FileAccess.Write : FileAccess.Read,
            FileShare.None,
            _bufferSize);

        this._diskService = new DiskService(this._stream);
        this._bufferManager = new BufferManager(this._diskService);
    }

    public static Engine Create(string filename)
    {
        return new Engine(filename, canWrite: true);
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
        if (this._stream !=null){
            //this._stream.Close();
            this._stream.Dispose();
        }
    }
}
