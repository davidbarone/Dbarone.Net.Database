using System.Xml;
using Dbarone.Net.Database;
using Dbarone.Net.Document;

public class BTreeManager
{
    private IDiskService DiskService { get; set; }
    public BTreeManager(IDiskService diskService)
    {
        this.DiskService = diskService;
    }

    public void Insert(int key, DocumentValue document)
    {

    }

    public void Delete(int key)
    {

    }

    public DocumentValue Search(int key)
    {
        return null;

    }
}