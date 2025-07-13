using System.Linq;
using Dbarone.Net.Database.Mapper;
using Dbarone.Net.Mapper;
using Xunit;

public class EnumerableMapperOperatorTests
{

    [Fact]
    public void MapGenericLists()
    {

        var conf = new MapperConfiguration()
            .RegisterType<int[]>()
            .RegisterType<float[]>()
            .RegisterType<int>()
            .RegisterType<float>()
            .RegisterOperator<ConvertibleMapperOperator>()
            .RegisterOperator<EnumerableMapperOperator>();

        ObjectMapper mapper = new ObjectMapper(conf);

        int[] ints = new int[] { 1, 2, 3, 4, 5 };
        var floats = mapper.Map<int[], float[]>(ints);
        Assert.Equal((float)1, floats.First());
    }
}