using Xunit;

namespace Dbarone.Net.Database.Tests;

public class BufferBaseTests
{
    [Fact]
    public void TestCreateBufferSize()
    {
        var buffer = new byte[] { 10, 20, 30 };
        var bb = new BufferBase(buffer);
        Assert.Equal(3, bb.Size);
    }

    [Fact]
    public void TestCreateBufferIndex()
    {
        var buffer = new byte[] { 10, 20, 30 };
        var bb = new BufferBase(buffer);
        Assert.Equal(10, bb[0]);
    }

    [Fact]
    public void TestBufferBaseRead()
    {
        var buffer = new byte[] { 10, 20, 30 };
        var bb = new BufferBase(buffer);
        Assert.Equal(30, bb.ReadByte(2));
    }

    [Fact]
    public void TestBufferBaseWrite()
    {
        var buffer = new byte[] { 10, 20, 30 };
        var bb = new BufferBase(buffer);
        bb.Write((byte)100, 2);
        Assert.Equal(100, bb[2]);
    }

}