namespace Dbarone.Net.Database.Tests;
using Xunit;
using Dbarone.Net.Database;
using System;

public class TypeTests{
    
    [Theory]
    // https://stackoverflow.com/questions/28514373/what-is-the-size-of-a-boolean-in-c-does-it-really-take-4-bytes
    [InlineData(typeof(bool),4)]
    [InlineData(typeof(byte),1)]
    [InlineData(typeof(sbyte),1)]
    [InlineData(typeof(char),1)]
    [InlineData(typeof(decimal),16)]
    [InlineData(typeof(double),8)]
    [InlineData(typeof(float),4)]
    [InlineData(typeof(int),4)]
    [InlineData(typeof(uint),4)]
    [InlineData(typeof(long),8)]
    [InlineData(typeof(ulong),8)]
    [InlineData(typeof(short),2)]
    [InlineData(typeof(ushort),2)]
    [InlineData(typeof(string),-1)]
    public void TestTypes_HaveCorrectSize(Type type, int expectedSize) {
        
        // Arrange

        // Act

        // Assert
        Assert.Equal(expectedSize, Types.GetByType(type).Size);
    }
}