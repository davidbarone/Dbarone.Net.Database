using Dbarone.Net.Database.Mapper;
using Xunit;

namespace Dbarone.Net.Database.Tests.Mapper;

public struct VectorStructA
{
    public int X { get; set; }
    public int Y { get; set; }
}

public struct VectorStructB
{
    public int X { get; set; }
    public int Y { get; set; }
}

public class MapStructsTests
{
    [Fact]
    public void Mapper_WhenMappingStructsDifferentTypes_ShouldMap()
    {
        var conf = new MapperConfiguration()
            .RegisterType<VectorStructA>()
            .RegisterType<VectorStructB>()
            .SetAutoRegisterTypes(true)
            .RegisterOperator<AssignableMapperOperator>()
            .RegisterOperator<MemberwiseMapperOperator>();
        var mapper = new ObjectMapper(conf);

        var v1 = new VectorStructA() { X = 3, Y = 4 };
        var v2 = mapper.Map<VectorStructA, VectorStructB>(v1);
        Assert.Equal(3, v2.X);
    }

    [Fact]
    public void Mapper_WhenRegisteringStructsSameTypes_ShouldMap()
    {
        var conf = new MapperConfiguration()
            .RegisterType<VectorStructA>()
            .RegisterOperator<AssignableMapperOperator>();
        var mapper = new ObjectMapper(conf);

        var v1 = new VectorStructA() { X = 3, Y = 4 };
        var v2 = mapper.Map<VectorStructA, VectorStructA>(v1);
        Assert.Equal(3, v2.X);
    }
}