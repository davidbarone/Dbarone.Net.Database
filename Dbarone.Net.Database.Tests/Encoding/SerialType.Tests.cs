using Xunit;
using System;

namespace Dbarone.Net.Database.Tests;

public class SerialTypeTests
{
    [Theory]
    [InlineData(DocumentType.Null, null, 0)]
    [InlineData(DocumentType.Boolean, null, 1)]
    [InlineData(DocumentType.Integer, null, 2)]
    [InlineData(DocumentType.Real, null, 3)]
    [InlineData(DocumentType.DateTime, null, 4)]
    [InlineData(DocumentType.Blob, 10, 26)]
    [InlineData(DocumentType.Text, 10, 27)]
    public void Test_SerialType(DocumentType DocumentType, int? length, int expected)
    {
        VarInt expectedVarInt = (VarInt)expected;

        // Create VarInt - #1
        var varInt = new SerialType(DocumentType, length);
        Assert.Equal((long)expectedVarInt.Value, (long)varInt.Value);

        // Create VarInt - #2
        varInt = new SerialType(expected);
        Assert.Equal(DocumentType, varInt.DocumentType);
        Assert.Equal(length, varInt.Length);
    }
}