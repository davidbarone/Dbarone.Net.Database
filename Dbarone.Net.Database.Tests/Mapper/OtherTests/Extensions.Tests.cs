using System;
using Dbarone.Net.Database.Mapper;
using Xunit;

namespace Dbarone.Net.Mapper.Tests;

public class ExtensionsTests
{
    [Fact]
    public void MapToExtensionMethods()
    {
        var str = "123";
        var actual = str.MapTo(typeof(int), new Type[] { typeof(ConvertibleMapperOperator) });
        Assert.IsType<int>(actual);
        Assert.Equal(123, actual);
    }
}