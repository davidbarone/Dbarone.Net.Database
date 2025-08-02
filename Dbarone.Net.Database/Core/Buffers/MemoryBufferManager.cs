using Dbarone.Net.Database;

public class MemoryBufferManager : BufferManager, IBufferManager
{
    IBuffer[] Pages = new GenericBuffer[0];

    public MemoryBufferManager(IPageHydrater pageHydrater, ITableSerializer tableSerializer, int? pageSize = null, TextEncoding textEncoding = TextEncoding.UTF8) : base(pageHydrater, tableSerializer, pageSize, textEncoding) { }

    public override int StoragePageCount()
    {
        return Pages.Count();
    }

    public override IBuffer StorageRead(int pageId)
    {
        return Pages[pageId];
    }

    public override void StorageWrite(IBuffer page)
    {
        var id = 0;// page.PageId;
        if (id >= Pages.Count())
        {
            Array.Resize(ref Pages, id);
        }
        Pages[id] = page;
    }
}