using Dbarone.Net.Database;

public class MemoryBufferManager : BufferManager, IBufferManager
{
    PageBuffer[] Pages = new PageBuffer[0];

    public MemoryBufferManager(int pageSize, ISerializer serializer) : base(pageSize, serializer) { }

    public override int StoragePageCount()
    {
        return Pages.Count();
    }

    public override PageBuffer StorageRead(int pageId)
    {
        return Pages[pageId];
    }

    public override void StorageWrite(PageBuffer page)
    {
        var id = page.PageId;
        if (id >= Pages.Count())
        {
            Array.Resize(ref Pages, id);
        }
        Pages[id] = page;
    }
}