
using System.Collections.Generic;
using Dbarone.Net.Database.Mapper;
using Dbarone.Net.Mapper;
using Xunit;

public class Customer
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = default!;
}

public class DictionaryTests
{

    [Fact]
    public void TestClassToDictionary()
    {
        var customer = new Customer()
        {
            CustomerId = 123,
            CustomerName = "Acme Ltd"
        };

        var conf = new MapperConfiguration()
            .SetAutoRegisterTypes(true)
            .RegisterOperator<AssignableMapperOperator>()
            .RegisterOperator<MemberwiseMapperOperator>();

        ObjectMapper mapper = new ObjectMapper(conf);

        var dict = mapper.Map<Customer, Dictionary<string, object>>(customer);
        Assert.Equal(123, dict["CustomerId"]);
    }

    [Fact]
    public void TestDictionaryToClass()
    {
        Dictionary<string, object> dict = new Dictionary<string, object>
        {
            {"CustomerId", 123 },
            {"CustomerName", "Acme Ltd" }
        };

        var conf = new MapperConfiguration()
            .SetAutoRegisterTypes(true)
            .RegisterOperator<AssignableMapperOperator>()
            .RegisterOperator<MemberwiseMapperDeferBuildOperator>();

        ObjectMapper mapper = new ObjectMapper(conf);
        var customer = mapper.Map<Dictionary<string, object>, Customer>(dict);
        Assert.Equal(123, customer.CustomerId);
    }
}