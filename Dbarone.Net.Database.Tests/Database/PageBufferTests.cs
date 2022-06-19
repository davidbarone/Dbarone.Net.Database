namespace Dbarone.Net.Database.Tests;
using Xunit;
using Dbarone.Net.Database;
using System;
using DbAssert = Dbarone.Net.Assertions.Assert;

public class PageBufferTests
{
    private const int testValue = 0b10101010;
    private const int testIndex = 100;

    [Fact]
    public void TestBuffer_Fill(){
        byte[] buffer = new byte[Page.PageSize];
        var pb = new PageBuffer(buffer, 0);
        pb.Fill(0, Page.PageSize, testValue);

        // This should not fail
        DbAssert.All(pb.ToArray(), (b) => b == testValue);
    }

    [Fact]
    public void TestBuffer_WriteBool()
    {
        // Arrange
        byte[] buffer = new byte[Page.PageSize];
        IPageBuffer pb = new PageBuffer(buffer, 0);
        pb.Write(true, testIndex);

        // Act
        var actual = pb.ReadBool(testIndex);

        // Assert
        Assert.Equal(true, actual);
    }

    [Fact]
    public void TestBuffer_WriteByte()
    {
        // Arrange
        byte[] buffer = new byte[Page.PageSize];
        IPageBuffer pb = new PageBuffer(buffer, 0);
        pb.Write((byte)testValue, testIndex);

        // Act
        var actual = pb.ReadByte(testIndex);

        // Assert
        Assert.Equal(testValue, actual);
    }

    [Fact]
    public void TestBuffer_WriteSByte()
    {
        // Arrange
        var value = (SByte)55;
        byte[] buffer = new byte[Page.PageSize];
        IPageBuffer pb = new PageBuffer(buffer, 0);
        pb.Write(value, testIndex);

        // Act
        var actual = pb.ReadSByte(testIndex);

        // Assert
        Assert.Equal(value, actual);
    }

}