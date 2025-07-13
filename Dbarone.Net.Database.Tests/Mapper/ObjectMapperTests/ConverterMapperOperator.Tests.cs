using Dbarone.Net.Database.Mapper;
using Dbarone.Net.Mapper;
using Xunit;

namespace Dbarone.Net.Mapper.Tests;

public class InClass
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public class OutClass
{
    public string FullName { get; set; }
}

public class ConverterMapperOperatorTests
{
    [Fact]
    public void TestConverter()
    {
        var mapper = new ObjectMapper(new MapperConfiguration()
            .SetAutoRegisterTypes(true)
            .RegisterOperator<ConverterMapperOperator>()
            .RegisterConverter<InClass, OutClass>((source) =>
            {
                return new OutClass
                {
                    FullName = source.FirstName + " " + source.LastName
                };
            })
        );

        // Do mapping
        var i = new InClass { FirstName = "John", LastName = "Doe" };
        var o = mapper.Map<InClass, OutClass>(i);

        Assert.Equal("John Doe", o.FullName);
    }
}