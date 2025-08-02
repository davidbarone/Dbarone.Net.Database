using System;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace Dbarone.Net.Database.Tests;

public class BufferManagerTests
{
    private int pageSize = 8192;


    public void BufferManager_Instantiate()
    {
        ITableSerializer tableSerializer = new TableSerializer();
        IPageHydrater pageHydrater = new PageHydrater();
        BufferManager bm = new MemoryBufferManager(pageSize, pageHydrater, tableSerializer, TextEncoding.UTF8);
        Assert.Equal(0, bm.MaxPageId);
        Assert.Equal(0, bm.Count);
    }

    public void BufferManager_CreatePage()
    {
        ITableSerializer tableSerializer = new TableSerializer();
        IPageHydrater pageHydrater = new PageHydrater();
        BufferManager bm = new MemoryBufferManager(pageSize, pageHydrater, tableSerializer, TextEncoding.UTF8);
        var page = bm.Create();
        Assert.Equal(PageType.Boot, page.PageType);
        Assert.Equal(1, bm.MaxPageId);
        Assert.Equal(1, bm.Count);
        Assert.Equal(0, bm.StoragePageCount());
    }

    public void BufferManager_StorageWrite()
    {
        ITableSerializer tableSerializer = new TableSerializer();
        IPageHydrater pageHydrater = new PageHydrater();
        BufferManager bm = new MemoryBufferManager(pageSize, pageHydrater, tableSerializer, TextEncoding.UTF8);
        var page = bm.Create();
        //bm.StorageWrite(bm.Serializer.Serialize(page));
        Assert.Equal(PageType.Boot, page.PageType);
        Assert.Equal(1, bm.MaxPageId);
        Assert.Equal(1, bm.Count);
        Assert.Equal(0, bm.StoragePageCount());
    }

    [Fact]
    public void TestCreateModifyAndGet()
    {
        ITableSerializer tableSerializer = new TableSerializer();
        IPageHydrater pageHydrater = new PageHydrater();
        BufferManager bm = new MemoryBufferManager(pageSize, pageHydrater, tableSerializer, TextEncoding.UTF8);

        var page0 = bm.Create();
        Assert.Equal(0, page0.PageId);
        var page1 = bm.Create();
        Assert.Equal(1, page1.PageId);
        var page2 = bm.Create();
        Assert.Equal(2, page2.PageId);
        page2.NextPageId = 345;

        // get page 2 again - next page should be 123
        var test = bm.Get(2);
        Assert.Equal(345, test.NextPageId);
    }
}