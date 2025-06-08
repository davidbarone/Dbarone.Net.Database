using System.Xml;
using Dbarone.Net.Database;
using Dbarone.Net.Document;

public class BTreeManager
{
    private BufferManager BufferManager { get; set; }
    public BTreeManager(BufferManager bufferManager)
    {
        this.BufferManager = bufferManager;
    }

    public void Insert(int key, TableCell document)
    {

    }

    public void Delete(int key)
    {

    }

    public TableCell Search(int key)
    {
        return null;

    }
}