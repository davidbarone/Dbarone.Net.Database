using Dbarone.Net.Database.Mapper;
using Dbarone.Net.Mapper;
using Xunit;

/// <summary>
/// Test class which offers implicit conversion from Int32 type.
/// </summary>
public class ImplicitOperatorTest
{
    public string StringValue { get; set; }

    public ImplicitOperatorTest()
    {
    }

    public ImplicitOperatorTest(int i)
    {
        StringValue = new string('X', i);
    }

    public static implicit operator ImplicitOperatorTest(int i) => new ImplicitOperatorTest(i);
}

public class ImplicitOperatorMapperTests
{
    [Fact]
    public void Map_Int_To_Float_Should_Map()
    {
        var conf = new MapperConfiguration()
            .RegisterType<int>()
            .RegisterType<ImplicitOperatorTest>()
            .RegisterOperator<ImplicitOperatorMapperOperator>();

        var mapper = new ObjectMapper(conf);

        int len = 10;
        var expected = new string('X', len);  // 'XXXXXXXXXX'
        var actual = mapper.Map<int, ImplicitOperatorTest>(len);
        Assert.Equal(expected, actual.StringValue);
    }
}