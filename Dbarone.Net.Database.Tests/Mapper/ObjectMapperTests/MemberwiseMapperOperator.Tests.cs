using Dbarone.Net.Database.Mapper;
using Dbarone.Net.Mapper;
using Xunit;

public class CustomerEntity
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; }
    public AddressEntity Address { get; set; }
}

public class CustomerDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; }
    public AddressDto Address { get; set; }
}

public class AddressEntity
{
    public int AddressId { get; set; }
    public string AddressLine1 { get; set; }
}

public class AddressDto
{
    public int AddressId { get; set; }
    public string AddressLine1 { get; set; }
}

public class MemberwiseMapperOperatorTests
{

    [Fact]
    public void MemberMapping()
    {
        var conf = new MapperConfiguration()
            .SetAutoRegisterTypes(true)
            .RegisterOperator<AssignableMapperOperator>()
            .RegisterOperator<MemberwiseMapperOperator>();
        var mapper = new ObjectMapper(conf);

        var addressA = new AddressEntity() { AddressId = 123, AddressLine1 = "Test Address" };
        var addressB = mapper.Map<AddressEntity, AddressDto>(addressA);
        Assert.Equal("Test Address", addressB.AddressLine1);
    }

    [Fact]
    public void NestedMemberMapping()
    {
        var conf = new MapperConfiguration()
            .SetAutoRegisterTypes(true)
            .RegisterOperator<AssignableMapperOperator>()
            .RegisterOperator<MemberwiseMapperOperator>();
        var mapper = new ObjectMapper(conf);

        var addressA = new AddressEntity() { AddressId = 123, AddressLine1 = "Test Address" };
        var customerA = new CustomerEntity() { CustomerId = 1, CustomerName = "Test Customer", Address = addressA };
        var customerB = mapper.Map<CustomerEntity, CustomerDto>(customerA);
        Assert.Equal("Test Address", customerB.Address.AddressLine1);
    }
}