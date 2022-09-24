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
        byte[] buffer = new byte[Global.PageSize];
        var pb = new PageBuffer(buffer, 0);
        pb.Fill(0, Global.PageSize, testValue);

        // This should not fail
        DbAssert.All(pb.ToArray(), (b) => b == testValue);
    }

    [Fact]
    public void TestBuffer_WriteBool()
    {
        // Arrange
        byte[] buffer = new byte[Global.PageSize];
        IBuffer pb = new PageBuffer(buffer, 0);
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
        byte[] buffer = new byte[Global.PageSize];
        IBuffer pb = new PageBuffer(buffer, 0);
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
        byte[] buffer = new byte[Global.PageSize];
        IBuffer pb = new PageBuffer(buffer, 0);
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
        byte[] buffer = new byte[Global.PageSize];
        IBuffer pb = new PageBuffer(buffer, 0);
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
        byte[] buffer = new byte[Global.PageSize];
        IBuffer pb = new PageBuffer(buffer, 0);
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
        byte[] buffer = new byte[Global.PageSize];
        IBuffer pb = new PageBuffer(buffer, 0);
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
        byte[] buffer = new byte[Global.PageSize];
        IBuffer pb = new PageBuffer(buffer, 0);
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
        byte[] buffer = new byte[Global.PageSize];
        IBuffer pb = new PageBuffer(buffer, 0);
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
        byte[] buffer = new byte[Global.PageSize];
        IBuffer pb = new PageBuffer(buffer, 0);
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
        byte[] buffer = new byte[Global.PageSize];
        IBuffer pb = new PageBuffer(buffer, 0);
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
        byte[] buffer = new byte[Global.PageSize];
        IBuffer pb = new PageBuffer(buffer, 0);
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
        byte[] buffer = new byte[Global.PageSize];
        IBuffer pb = new PageBuffer(buffer, 0);
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
        byte[] buffer = new byte[Global.PageSize];
        IBuffer pb = new PageBuffer(buffer, 0);
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
        byte[] buffer = new byte[Global.PageSize];
        IBuffer pb = new PageBuffer(buffer, 0);
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
        var value = Guid.NewGuid();;
        byte[] buffer = new byte[Global.PageSize];
        IBuffer pb = new PageBuffer(buffer, 0);
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
        byte[] buffer = new byte[Global.PageSize];
        IBuffer pb = new PageBuffer(buffer, 0);
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
        var value = new byte[8] {55,55,55,55,55,55,55,55};
        byte[] buffer = new byte[Global.PageSize];
        IBuffer pb = new PageBuffer(buffer, 0);
        pb.Write(value, testIndex);

        // Act
        var actual = pb.ReadBytes(testIndex, value.Length);

        // Assert
        Assert.Equal(value, actual);
    }
}