using Dbarone.Net.Database;

public class MemoryBufferManager : BufferManager, IBufferManager
{
    GenericBuffer[] Pages = new GenericBuffer[0];

    public MemoryBufferManager(int pageSize/*, ISerializer serializer*/) : base(pageSize/*, serializer*/) { }

    public override int StoragePageCount()
    {
        return Pages.Count();
    }

    public override GenericBuffer StorageRead(int pageId)
    {
        return Pages[pageId];
    }

    public override void StorageWrite(GenericBuffer page)
    {
        var id = 0;// page.PageId;
        if (id >= Pages.Count())
        {
            Array.Resize(ref Pages, id);
        }
        Pages[id] = page;
    }
}