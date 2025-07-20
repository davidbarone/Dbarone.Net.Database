using Dbarone.Net.Database;
using Dbarone.Net.Assertions;

namespace Dbarone.Net.Database;

public class DiskBufferManager : BufferManager, IBufferManager
{
    private Stream Stream;

    public DiskBufferManager(Stream stream, int pageSize, IPageHydrater pageHydrater, ITableSerializer tableSerializer, TextEncoding textEncoding = TextEncoding.UTF8) : base(pageSize, pageHydrater, tableSerializer, textEncoding)
    {
        this.Stream = stream;
    }

    public override long StoragePageCount()
    {
        return (this.Stream.Length / PageSize);
    }

    /// <summary>
    /// Physical read from store.
    /// </summary>
    /// <param name="pageId">The page id.</param>
    /// <returns>Returns the page buffer.</returns>
    public override IBuffer StorageRead(int pageId)
    {
        byte[] buffer = new byte[Global.PageSize];
        int start = (pageId * Global.PageSize);
        int length = Global.PageSize;
        this.Stream.Position = start;
        int read = this.Stream.Read(buffer, 0, length);
        var pb = new GenericBuffer(buffer);

        // Ensure page id is correct.
        //Assert.Equals(pb.PageId, pageId);
        return pb;
    }

    /// <summary>
    /// Physical write to store.
    /// </summary>
    /// <param name="page">The page to write.</param>
    public override void StorageWrite(IBuffer page)
    {
        var buffer = page.ToArray();
        Assert.Equals(buffer.Length, this.PageSize);
        var start = 0;// (page.PageId * this.PageSize);
        this.Stream.Position = start;
        this.Stream.Write(buffer);
    }
}