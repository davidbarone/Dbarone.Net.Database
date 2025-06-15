using System;
using Xunit;

namespace Dbarone.Net.Database.Tests;

public class PageBufferTests
{
    private const int testValue = 0b10101010;
    private const int testIndex = 100;  // to delete?
    private int pageSize = 8192;

    [Fact]
    public void TestCreateBufferSize()
    {
        var buffer = new byte[] { 10, 20, 30 };
        var pb = new PageBuffer(buffer);
        Assert.Equal(3, pb.Length);
    }

    [Fact]
    public void TestCreateBufferIndex()
    {
        var buffer = new byte[] { 10, 20, 30 };
        var pb = new PageBuffer(buffer);
        Assert.Equal(10, pb[0]);
    }


    [Fact]
    public void TestBufferFill()
    {
        var buffer = new byte[] { 10, 20, 30 };
        var pb = new PageBuffer(buffer);
        pb.Fill(0, 3, (byte)100);
        Assert.Equal(3, pb.Length);
        Assert.Equal(100, pb[0]);
        Assert.Equal(100, pb[1]);
        Assert.Equal(100, pb[2]);
    }

    [Fact]
    public void TestBufferClear()
    {
        var buffer = new byte[] { 10, 20, 30 };
        var pb = new PageBuffer(buffer);
        pb.Clear(0, 3);
        Assert.Equal(3, pb.Length);
        Assert.Equal(0, pb[0]);
        Assert.Equal(0, pb[1]);
        Assert.Equal(0, pb[2]);
    }

    [Fact]
    public void TestBufferSlice()
    {
        var buffer = new byte[] { 10, 20, 30, 40, 50, 60, 70, 80 };
        var pb = new PageBuffer(buffer);
        var slice = pb.Slice(2, 3);
        Assert.Equal(8, pb.Length);
        Assert.Equal(30, slice[0]);
        Assert.Equal(40, slice[1]);
        Assert.Equal(50, slice[2]);
    }

    [Fact]
    public void TestBuffer_Fill()
    {
        byte[] buffer = new byte[pageSize];
        var pb = new PageBuffer(buffer);
        pb.Fill(0, pageSize, testValue);
        Assert.Equal(pageSize, pb.Length);
        Assert.All(pb.ToArray(), (b) => Assert.Equal(testValue, b));
    }

    [Fact]
    public void TestBuffer_WriteBool()
    {
        // Arrange
        byte[] buffer = new byte[pageSize];
        IBuffer pb = new PageBuffer(buffer);
        pb.Write(true);

        // Act
        var actual = pb.ReadBool();

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void TestBuffer_WriteDouble()
    {
        // Arrange
        var value = (double)123.45;
        byte[] buffer = new byte[pageSize];
        IBuffer pb = new PageBuffer(buffer);
        pb.Write(value);

        // Act
        var actual = pb.ReadDouble();

        // Assert
        Assert.Equal(value, actual);
    }

    [Fact]
    public void TestBuffer_WriteInt64()
    {
        // Arrange
        var value = (Int64)12345;
        byte[] buffer = new byte[pageSize];
        IBuffer pb = new PageBuffer(buffer);
        pb.Write(value);

        // Act
        var actual = pb.ReadInt64();

        // Assert
        Assert.Equal(value, actual);
    }

    [Fact]
    public void TestBuffer_WriteDateTime()
    {
        // Arrange
        var value = DateTime.Now.Date;
        byte[] buffer = new byte[pageSize];
        IBuffer pb = new PageBuffer(buffer);
        pb.Write(value);

        // Act
        var actual = pb.ReadDateTime();

        // Assert
        Assert.Equal(value, actual);
    }

    [Fact]
    public void TestBuffer_WriteString()
    {
        // Arrange
        var value = "foo bar";
        byte[] buffer = new byte[pageSize];
        IBuffer pb = new PageBuffer(buffer);
        pb.Write(value);

        // Act
        var actual = pb.ReadString(value.Length);

        // Assert
        Assert.Equal(value, actual);
    }

    [Fact]
    public void TestBuffer_WriteBlob()
    {
        // Arrange
        var value = new byte[8] { 55, 55, 55, 55, 55, 55, 55, 55 };
        byte[] buffer = new byte[pageSize];
        IBuffer pb = new PageBuffer(buffer);
        pb.Write(value);

        // Act
        var actual = pb.ReadBytes(value.Length);

        // Assert
        Assert.Equal(value, actual);
    }
}