using Dbarone.Net.Database.Mapper;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Xunit;

namespace Dbarone.Net.Mapper;

public class EventsTests
{
    [Fact]
    public void TestBuildEvent()
    {
        // Given
        var conf = new MapperConfiguration()
            .SetAutoRegisterTypes(true)
            .RegisterOperator<ConvertibleMapperOperator>();

        var mapper = new ObjectMapper(conf);

        int a = 1;
        var buildCount = 0;
        MapperOperatorLogDelegate onLog = (MapperOperator mapperOperator, MapperOperatorLogType logType) =>
        {
            if (logType == MapperOperatorLogType.Build)
            {
                buildCount++;
            }
        };
        mapper.OnLog = onLog;
        var b = mapper.Map<int, float>(a);  // Will automatically register necessary types here
        Assert.Equal((float)1, b);
        Assert.Equal(1, buildCount);
    }
}