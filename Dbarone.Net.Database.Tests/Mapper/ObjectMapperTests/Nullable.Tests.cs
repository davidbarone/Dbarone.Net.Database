using System.Collections.Generic;
using Dbarone.Net.Database.Mapper;
using Dbarone.Net.Mapper;
using Xunit;

namespace Dbarone.Net.Mapper.Tests;

public class NullableMember
{
    public int? Age { get; set; } = 20;
    public int? Category { get; set; } = null;
}

public class NullableTests
{
    [Fact]
    public void NullableInt()
    {
        int? a = 123;
        var conf = new MapperConfiguration()
            .SetAutoRegisterTypes(true)
            .RegisterOperator<AssignableMapperOperator>()
            .RegisterOperator<NullableSourceMapperOperator>();
        var mapper = new ObjectMapper(conf);

        var b = mapper.Map<int?, int>(a);
        Assert.IsType<int>(b);
        Assert.Equal(123, b);
    }

    [Fact]
    public void NullableMember()
    {
        NullableMember a = new NullableMember();

        var conf = new MapperConfiguration()
            .SetAutoRegisterTypes(true)
            .RegisterOperator<AssignableMapperOperator>()
            .RegisterOperator<MemberwiseMapperOperator>();
        var mapper = new ObjectMapper(conf);

        var b = mapper.Map<NullableMember, Dictionary<string, object>>(a);
        Assert.IsType<Dictionary<string, object>>(b);
        Assert.Equal(20, b["Age"]);
        Assert.Null(b["Category"]);
    }
}