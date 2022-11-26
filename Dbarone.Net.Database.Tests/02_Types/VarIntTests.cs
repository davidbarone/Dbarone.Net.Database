namespace Dbarone.Net.Database.Tests;
using Xunit;
using Dbarone.Net.Database;
using System;

public class VarIntTests
{
    [Theory]
    [InlineData(0, 1, new byte[] { 0 })]
    [InlineData(127, 1, new byte[] { 0x7F })]
    [InlineData(128, 2, new byte[] { 0x81, 0x00 })]
    [InlineData(8192, 2, new byte[] { 0xC0, 0x00 })]
    [InlineData(16383, 2, new byte[] { 0xFF, 0x7F })]
    [InlineData(16384, 3, new byte[] { 0x81, 0x80, 0x00 })]
    [InlineData(2097151, 3, new byte[] { 0xFF, 0xFF, 0x7F })]
    [InlineData(2097152, 4, new byte[] { 0x81, 0x80, 0x80, 0x00 })]
    [InlineData(134217728, 4, new byte[] { 0xC0, 0x80, 0x80, 0x00 })]
    [InlineData(268435455, 4, new byte[] { 0xFF, 0xFF, 0xFF, 0x7F })]
    public void TestVarInt_WithIntConstructor(int value, int expectedLength, byte[] expectedBytes)
    {
        var varInt = new VarInt(value);
        Assert.Equal(expectedLength, varInt.Length);
        Assert.Equal(expectedBytes, varInt.Bytes);
        Assert.Equal(varInt.Value, value);
        Assert.Equal(varInt.Length, varInt.Bytes.Length);
    }

    [Theory]
    [InlineData(new byte[] { 0 }, 0, 1)]
    [InlineData(new byte[] { 0x7F }, 127, 1)]
    [InlineData(new byte[] { 0x81, 0x00 }, 128, 2)]
    [InlineData(new byte[] { 0xC0, 0x00 }, 8192, 2)]
    [InlineData(new byte[] { 0xFF, 0x7F }, 16383, 2)]
    [InlineData(new byte[] { 0x81, 0x80, 0x00 }, 16384, 3)]
    [InlineData(new byte[] { 0xFF, 0xFF, 0x7F }, 2097151, 3)]
    [InlineData(new byte[] { 0x81, 0x80, 0x80, 0x00 }, 2097152, 4)]
    [InlineData(new byte[] { 0xC0, 0x80, 0x80, 0x00 }, 134217728, 4)]
    [InlineData(new byte[] { 0xFF, 0xFF, 0xFF, 0x7F }, 268435455, 4)]
    public void TestVarInt_WithByteArrayConstructor(byte[] bytes, int expectedValue, int expectedLength)
    {
        var varInt = new VarInt(bytes);
        Assert.Equal(expectedLength, varInt.Length);
        Assert.Equal(expectedValue, varInt.Value);
        Assert.Equal(varInt.Bytes, bytes);
    }
}