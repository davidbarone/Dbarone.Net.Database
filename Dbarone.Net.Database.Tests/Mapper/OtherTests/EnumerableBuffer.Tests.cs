using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Dbarone.Net.Extensions;
using Xunit;

public class EnumerableBufferTests
{
    [Fact]
    public void TestToArrayList()
    {
        object[] arr = new object[] { 1, 2, 3, 4, 5 };
        int expectedLength = arr.Length;
        var buffer = new EnumerableBuffer(arr);
        var converted = buffer.ToArrayList();
        Assert.Equal(expectedLength, converted.Count);
        Assert.IsType<ArrayList>(converted);
    }

    [Fact]
    public void TestToQueue()
    {
        object[] arr = new object[] { 1, 2, 3, 4, 5 };
        int expectedLength = arr.Length;
        var buffer = new EnumerableBuffer(arr);
        var converted = buffer.ToQueue();
        Assert.Equal(expectedLength, converted.Count);
        Assert.IsType<Queue>(converted);
    }

    [Fact]
    public void TestToStack()
    {
        object[] arr = new object[] { 1, 2, 3, 4, 5 };
        int expectedLength = arr.Length;
        var buffer = new EnumerableBuffer(arr);
        var converted = buffer.ToStack();
        Assert.Equal(expectedLength, converted.Count);
        Assert.IsType<Stack>(converted);
    }

    [Fact]
    public void TestToGenericList()
    {
        object[] arr = new object[] { 1, 2, 3, 4, 5 };
        int expectedLength = arr.Length;
        var buffer = new EnumerableBuffer(arr);
        var converted = buffer.ToGenericList<int>();
        Assert.Equal(expectedLength, converted.Count);
        Assert.IsType<List<int>>(converted);
    }

    [Fact]
    public void TestToGenericEnumerable()
    {
        object[] arr = new object[] { 1, 2, 3, 4, 5 };
        int expectedLength = arr.Length;
        var buffer = new EnumerableBuffer(arr);
        var converted = buffer.ToGenericEnumerable<int>();
        Assert.Equal(expectedLength, converted.Count());
        Assert.IsAssignableFrom<IEnumerable<int>>(converted);
    }

    [Fact]
    public void TestEnumerableBufferWithMapper()
    {
        object[] arr = new object[] { 1, 2, 3, 4, 5 };
        // Mapper converts int to DateTime
        MapperDelegate mapper = (s) => { return DateTime.Now.AddDays((int)s!); };
        int expectedLength = arr.Length;
        var buffer = new EnumerableBuffer(arr, mapper);
        var converted = buffer.ToArray<DateTime>();
        Assert.Equal(expectedLength, converted.Count());
        Assert.IsType<DateTime[]>(converted);
    }

    [Fact]
    public void TestTo()
    {
        object[] arr = new object[] { 1, 2, 3, 4, 5 };
        int expectedLength = arr.Length;
        var buffer = new EnumerableBuffer(arr);
        var converted = buffer.To<Stack>();
        Assert.Equal(expectedLength, converted.Count);
        Assert.IsAssignableFrom<Stack>(converted);
    }
}