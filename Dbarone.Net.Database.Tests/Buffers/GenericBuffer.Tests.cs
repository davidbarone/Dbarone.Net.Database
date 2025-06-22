using System;
using Xunit;

namespace Dbarone.Net.Database;

public class GenericBufferTests
{

    [Fact]
    public void TestCreateResizableBuffer()
    {
        var buf = new GenericBuffer();
        Assert.Equal(0, buf.Position);
        Assert.Equal(0, buf.Length);
    }

    [Fact]
    public void TestResizableBuffer_Write()
    {
        var buf = new GenericBuffer();
        var length = buf.Write((long)123);
        Assert.Equal(length, buf.Position);
        Assert.Equal(length, buf.Length);
    }

    [Fact]
    public void TestResizableBuffer_SetPosition()
    {
        var buf = new GenericBuffer();
        //buf.Write((long)123);
        buf.Position = 100;
        Assert.Equal(100, buf.Position);
        var length = buf.Write((long)123);
        Assert.Equal(100 + length, buf.Position);
        Assert.Equal(100 + length, buf.Length);
    }

    [Fact]
    public void TestResizableBuffer_WriteRead()
    {
        var buf = new GenericBuffer();
        var length = buf.Write((long)123);
        Assert.Equal(length, buf.Position);
        Assert.Equal(length, buf.Length);

        // reset position + read int
        buf.Position = 0;
        var actual = buf.ReadInt64();
        Assert.Equal(123, actual);
    }

    [Theory]
    [InlineData(DocumentType.Boolean, true)]
    [InlineData(DocumentType.Integer, long.MaxValue)]
    [InlineData(DocumentType.Real, double.MaxValue)]
    [InlineData(DocumentType.Text, "foobar")]
    [InlineData(DocumentType.Blob, new byte[] { 1, 2, 3, 4, 5 })]
    public void WriteTests(DocumentType docType, object expectedValue)
    {
        var buf = new GenericBuffer();
        buf.Write(expectedValue);
        buf.Position = 0;
        var actualValue = buf.Read(docType);
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void WriteDateTimeTest()
    {
        // Arrange
        var value = DateTime.Now;
        var buf = new GenericBuffer();
        buf.Write(value);

        // Act
        buf.Position = 0;
        var actual = buf.Read(DocumentType.DateTime);

        // Assert
        Assert.Equal(value, actual);
    }

    [Fact]
    public void TestBuffer_WriteStringUnicode()
    {
        // Arrange
        var value = "Þð©á";
        var buf = new GenericBuffer();
        var length = buf.Write(value);

        // Act
        buf.Position = 0;
        var actual = buf.Read(DocumentType.Text, length);

        // Assert
        Assert.Equal(value, actual);
    }
}