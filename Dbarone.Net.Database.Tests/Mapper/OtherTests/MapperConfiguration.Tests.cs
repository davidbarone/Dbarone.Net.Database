using System.Linq;
using Dbarone.Net.Database.Mapper;
using Dbarone.Net.Mapper;
using Xunit;

public class MapperConfigurationTests
{
    [Fact]
    public void TestAutoRegisterTypes()
    {
        // Given
        var conf = new MapperConfiguration()
            .SetAutoRegisterTypes(true)                // No need to register individual types here.
            .RegisterOperator<ConvertibleMapperOperator>()
            .RegisterOperator<EnumerableMapperOperator>();
        var mapper = new ObjectMapper(conf);

        int[] ints = new int[] { 1, 2, 3, 4, 5 };
        var floats = mapper.Map<int[], float[]>(ints);  // Will automatically register necessary types here
        Assert.Equal((float)1, floats.First());
    }
}