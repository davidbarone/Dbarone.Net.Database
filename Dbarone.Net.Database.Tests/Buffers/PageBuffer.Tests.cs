using System;
using Xunit;

namespace Dbarone.Net.Database.Tests;

public class PageBufferTests
{
    private const int testValue = 0b10101010;
    private const int testIndex = 100;
    private int pageSize = 8192;

    [Fact]
    public void TestCreateBufferSize()
    {
        var buffer = new byte[] { 10, 20, 30 };
        var pb = new PageBuffer(buffer);
        Assert.Equal(3, pb.Size);
    }

    [Fact]
    public void TestCreateBufferIndex()
    {
        var buffer = new byte[] { 10, 20, 30 };
        var pb = new PageBuffer(buffer);
        Assert.Equal(10, pb[0]);
    }

    [Fact]
    public void TestBufferRead()
    {
        var buffer = new byte[] { 10, 20, 30 };
        var pb = new PageBuffer(buffer);
        Assert.Equal(30, pb.ReadByte(2));
    }

    [Fact]
    public void TestBufferWrite()
    {
        var buffer = new byte[] { 10, 20, 30 };
        var pb = new PageBuffer(buffer);
        pb.Write((byte)100, 2);
        Assert.Equal(100, pb[2]);
    }

    [Fact]
    public void TestBufferFill()
    {
        var buffer = new byte[] { 10, 20, 30 };
        var pb = new PageBuffer(buffer);
        pb.Fill(0, 3, (byte)100);
        Assert.Equal(3, pb.Size);
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
        Assert.Equal(3, pb.Size);
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
        Assert.Equal(8, pb.Size);
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
        Assert.Equal(pageSize, pb.Size);
        Assert.All(pb.ToArray(), (b) => Assert.Equal(testValue, b));
    }

    [Fact]
    public void TestBuffer_WriteBool()
    {
        // Arrange
        byte[] buffer = new byte[pageSize];
        IBuffer pb = new PageBuffer(buffer);
        pb.Write(true, testIndex);

        // Act
        var actual = pb.ReadBool(testIndex);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void TestBuffer_WriteByte()
    {
        // Arrange
        byte[] buffer = new byte[pageSize];
        IBuffer pb = new PageBuffer(buffer);
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
        var value = (sbyte)55;
        byte[] buffer = new byte[pageSize];
        IBuffer pb = new PageBuffer(buffer);
        pb.Write(value, testIndex);

        // Act
        var actual = pb.ReadSByte(testIndex);

        // Assert
        Assert.Equal(value, actual);
    }

    [Fact]
    public void TestBuffer_WriteChar()
    {
        // Arrange
        var value = (char)55;
        byte[] buffer = new byte[pageSize];
        IBuffer pb = new PageBuffer(buffer);
        pb.Write(value, testIndex);

        // Act
        var actual = pb.ReadChar(testIndex);

        // Assert
        Assert.Equal(value, actual);
    }

    [Fact]
    public void TestBuffer_WriteDecimal()
    {
        // Arrange
        var value = (decimal)123.45;
        byte[] buffer = new byte[pageSize];
        IBuffer pb = new PageBuffer(buffer);
        pb.Write(value, testIndex);

        // Act
        var actual = pb.ReadDecimal(testIndex);

        // Assert
        Assert.Equal(value, actual);
    }

    [Fact]
    public void TestBuffer_WriteDouble()
    {
        // Arrange
        var value = (double)123.45;
        byte[] buffer = new byte[pageSize];
        IBuffer pb = new PageBuffer(buffer);
        pb.Write(value, testIndex);

        // Act
        var actual = pb.ReadDouble(testIndex);

        // Assert
        Assert.Equal(value, actual);
    }

    [Fact]
    public void TestBuffer_WriteSingle()
    {
        // Arrange
        var value = (Single)123.45;
        byte[] buffer = new byte[pageSize];
        IBuffer pb = new PageBuffer(buffer);
        pb.Write(value, testIndex);

        // Act
        var actual = pb.ReadSingle(testIndex);

        // Assert
        Assert.Equal(value, actual);
    }

    [Fact]
    public void TestBuffer_WriteInt16()
    {
        // Arrange
        var value = (Int16)12345;
        byte[] buffer = new byte[pageSize];
        IBuffer pb = new PageBuffer(buffer);
        pb.Write(value, testIndex);

        // Act
        var actual = pb.ReadInt16(testIndex);

        // Assert
        Assert.Equal(value, actual);
    }

    [Fact]
    public void TestBuffer_WriteUInt16()
    {
        // Arrange
        var value = (UInt16)12345;
        byte[] buffer = new byte[pageSize];
        IBuffer pb = new PageBuffer(buffer);
        pb.Write(value, testIndex);

        // Act
        var actual = pb.ReadUInt16(testIndex);

        // Assert
        Assert.Equal(value, actual);
    }

    [Fact]
    public void TestBuffer_WriteInt32()
    {
        // Arrange
        var value = (Int32)12345;
        byte[] buffer = new byte[pageSize];
        IBuffer pb = new PageBuffer(buffer);
        pb.Write(value, testIndex);

        // Act
        var actual = pb.ReadInt32(testIndex);

        // Assert
        Assert.Equal(value, actual);
    }

    [Fact]
    public void TestBuffer_WriteUInt32()
    {
        // Arrange
        var value = (UInt32)12345;
        byte[] buffer = new byte[pageSize];
        IBuffer pb = new PageBuffer(buffer);
        pb.Write(value, testIndex);

        // Act
        var actual = pb.ReadUInt32(testIndex);

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
        pb.Write(value, testIndex);

        // Act
        var actual = pb.ReadInt64(testIndex);

        // Assert
        Assert.Equal(value, actual);
    }

    [Fact]
    public void TestBuffer_WriteUInt64()
    {
        // Arrange
        var value = (UInt64)12345;
        byte[] buffer = new byte[pageSize];
        IBuffer pb = new PageBuffer(buffer);
        pb.Write(value, testIndex);

        // Act
        var actual = pb.ReadUInt64(testIndex);

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
        pb.Write(value, testIndex);

        // Act
        var actual = pb.ReadDateTime(testIndex);

        // Assert
        Assert.Equal(value, actual);
    }

    [Fact]
    public void TestBuffer_WriteGuid()
    {
        // Arrange
        var value = Guid.NewGuid(); ;
        byte[] buffer = new byte[pageSize];
        IBuffer pb = new PageBuffer(buffer);
        pb.Write(value, testIndex);

        // Act
        var actual = pb.ReadGuid(testIndex);

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
        pb.Write(value, testIndex);

        // Act
        var actual = pb.ReadString(testIndex, value.Length);

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
        pb.Write(value, testIndex);

        // Act
        var actual = pb.ReadBytes(testIndex, value.Length);

        // Assert
        Assert.Equal(value, actual);
    }
}