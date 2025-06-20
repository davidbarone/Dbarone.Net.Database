using Dbarone.Net.Database;
using Xunit;

namespace Dbarone.Net.Document.Tests;

public class ZigZagTests
{
    [Theory]
    [InlineData(-1, 18446744073709551615)]
    [InlineData(-1, 18446744073709551488)]
    public void TestLongToUlongCast(long input, ulong output)
    {
        // note that casting a small negative number produces a large unsigned number
        // This is what standard .NET casting does - however, does not produce small
        // number which we need for ZigZag compression.
        Assert.Equal(output, (ulong)input);
    }

    [Theory]
    [InlineData(-10, 19)]
    [InlineData(-9, 17)]
    [InlineData(-8, 15)]
    [InlineData(-7, 13)]
    [InlineData(-6, 11)]
    [InlineData(-5, 9)]
    [InlineData(-4, 7)]
    [InlineData(-3, 5)]
    [InlineData(-2, 3)]
    [InlineData(-1, 1)]
    [InlineData(0, 0)]
    [InlineData(1, 2)]
    [InlineData(2, 4)]
    [InlineData(3, 6)]
    [InlineData(4, 8)]
    [InlineData(5, 10)]
    [InlineData(6, 12)]
    [InlineData(7, 14)]
    [InlineData(8, 16)]
    [InlineData(9, 18)]
    [InlineData(10, 20)]
    public void TestZigZagEncoding(long actualInput, ulong expectedOutput)
    {
        // all positive integers should become even integers twice as big
        // all negative integers should become positive odd integers, 1 less than double
        ZigZag zz = new ZigZag(actualInput);
        Assert.Equal(expectedOutput, zz.Encoded);
    }
}