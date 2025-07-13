using Dbarone.Net.Database.Mapper;
using Dbarone.Net.Mapper;
using Xunit;

public class AssignableMapperOperatorTests
{

    [Fact]
    public void MapSameTypes()
    {
        var mapper = new ObjectMapper(new MapperConfiguration()
            .RegisterType<int>()
            .RegisterOperator<AssignableMapperOperator>()
        );

        var a = 123;
        var b = mapper.Map<int, int>(a);
        Assert.Equal(a, b);
    }

    [Fact]
    public void MapValueToNullable()
    {
        var mapper = new ObjectMapper(new MapperConfiguration()
            .RegisterType<int>()
            .RegisterType<int?>()
            .RegisterOperator<AssignableMapperOperator>()
        );

        var a = 123;
        var b = mapper.Map<int, int?>(a);
        Assert.Equal(a, b.Value);
    }
}