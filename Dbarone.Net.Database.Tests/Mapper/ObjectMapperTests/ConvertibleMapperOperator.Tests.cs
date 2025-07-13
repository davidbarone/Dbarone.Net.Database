using Dbarone.Net.Database.Mapper;
using Dbarone.Net.Mapper;
using Xunit;

public class ConvertibleMapperOperatorTests
{
    [Fact]
    public void Map_Int_To_Float_Should_Map()
    {

        var conf = new MapperConfiguration()
            .RegisterType<int>()
            .RegisterType<long>()
            .RegisterOperator<ConvertibleMapperOperator>();

        var mapper = new ObjectMapper(conf);

        var a = 123;
        var b = mapper.Map<int, long>(a);
        Assert.Equal((long)a, b);
    }
}