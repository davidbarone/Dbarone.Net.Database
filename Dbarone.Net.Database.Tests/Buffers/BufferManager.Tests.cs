using System;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace Dbarone.Net.Database.Tests;

public class BufferManagerTests
{
    private int pageSize = 8192;


    public void BufferManager_Instantiate()
    {
        ISerializer serializer = new Serializer(1024, Document.TextEncoding.UTF8);
        BufferManager bm = new MemoryBufferManager(pageSize, serializer);
        Assert.Equal(0, bm.MaxPageId);
        Assert.Equal(0, bm.Count);
    }

    public void BufferManager_CreatePage()
    {
        ISerializer serializer = new Serializer(1024, Document.TextEncoding.UTF8);
        BufferManager bm = new MemoryBufferManager(pageSize, serializer);
        var page = bm.Create(PageType.Boot);
        Assert.Equal(PageType.Boot, page.PageType);
        Assert.Equal(1, bm.MaxPageId);
        Assert.Equal(1, bm.Count);
        Assert.Equal(0, bm.StoragePageCount());
    }

    public void BufferManager_StorageWrite()
    {
        ISerializer serializer = new Serializer(1024, Document.TextEncoding.UTF8);
        BufferManager bm = new MemoryBufferManager(pageSize, serializer);
        var page = bm.Create(PageType.Boot);
        bm.StorageWrite(bm.Serializer.Serialize(page));
        Assert.Equal(PageType.Boot, page.PageType);
        Assert.Equal(1, bm.MaxPageId);
        Assert.Equal(1, bm.Count);
        Assert.Equal(0, bm.StoragePageCount());
    }
}