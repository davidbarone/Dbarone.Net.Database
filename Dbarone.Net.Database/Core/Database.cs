namespace Dbarone.Net.Database;
public class Database : IDatabase
{
    private string _filename;
    private Stream _stream;
    private int _bufferSize = 8192;

    public Database(string filename, bool canWrite)
    {
        this._filename = filename;
        var exists = File.Exists(this._filename);
        this._stream = new FileStream(
            filename,
            exists ? FileMode.Open : FileMode.OpenOrCreate,
            canWrite ? FileAccess.Write : FileAccess.Read,
            FileShare.None,
            _bufferSize);
    }

    public static Database Create(string filename)
    {
        return new Database(filename, canWrite: true);
    }

    public static Database Open(string filename, bool canWrite)
    {
        return new Database(filename, canWrite: true);
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
