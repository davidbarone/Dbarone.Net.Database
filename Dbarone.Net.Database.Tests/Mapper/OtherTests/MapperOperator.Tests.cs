using System;
using Xunit;
using Dbarone.Net.Database.Mapper;
using System.Collections.Generic;

namespace Dbarone.Net.Database.Tests.Mapper;

public class Company
{
    public int CompanyId { get; set; }
    public string CompanyName { get; set; }
    public Person[] Employees { get; set; }
}

public class CompanyWithDeferBuild
{
    public int CompanyId { get; set; }
    public string CompanyName { get; set; }
    public dynamic Employees { get; set; }  // This property will be filled with Person[], but we declare as dynamic to test lazy operator build.
}

public class Person
{
    public int PersonId { get; set; }
    public string PersonName { get; set; }
    public DateTime DoB { get; set; }
}

public class CompanyDto
{
    public int CompanyId { get; set; }
    public string CompanyName { get; set; }
    public List<PersonDto> Employees { get; set; }
}

public class PersonDto
{
    public int PersonId { get; set; }
    public string PersonName { get; set; }
    public DateTime DoB { get; set; }
}


public class MapperOperatorTests
{
    [Fact]
    public void GetOperator1()
    {
        var conf = new MapperConfiguration()
            .RegisterType<int>()
            .RegisterType<float>()
            .RegisterOperator<ConvertibleMapperOperator>();
        var mapper = new ObjectMapper(conf);

        var op = mapper.GetMapperOperator<int, float>();
        Assert.NotNull(op);
    }

    [Fact]
    public void TestPrettyPrint()
    {
        Company[] companies = new Company[] {
        new Company {
            CompanyId=123,
            CompanyName="Acme Ltd",
            Employees = new Person[]{
                new Person { PersonId = 1, PersonName = "Wily C.", DoB=new DateTime(1990,01,01)},
                new Person { PersonId = 2, PersonName = "R.Runner", DoB=new DateTime(1991,07,01)},
            }
        },

        new Company {
            CompanyId=456,
            CompanyName="Globex Corporation",
            Employees = new Person[]{
                new Person { PersonId = 1, PersonName = "Homer", DoB=new DateTime(1990,01,01)},
                new Person { PersonId = 2, PersonName = "Marge", DoB=new DateTime(1991,07,01)},
            }
        } };

        var conf = new MapperConfiguration()
            .SetAutoRegisterTypes(true)
            .RegisterOperator<AssignableMapperOperator>()
            .RegisterOperator<MemberwiseMapperOperator>()
            .RegisterOperator<EnumerableMapperOperator>();
        var mapper = new ObjectMapper(conf);
        var objectMapper = new ObjectMapper(conf);
        var op = objectMapper.GetMapperOperator(typeof(Company[]), typeof(List<CompanyDto>));
        var str = op.PrettyPrint();

        Assert.Equal(@"+- EnumerableMapperOperator (Company[]->List`1)
   +- []: MemberwiseMapperOperator (Company->CompanyDto)
      +- CompanyId: AssignableMapperOperator (Int32->Int32)
      +- CompanyName: AssignableMapperOperator (String->String)
      +- Employees: EnumerableMapperOperator (Person[]->List`1)
         +- []: MemberwiseMapperOperator (Person->PersonDto)
            +- PersonId: AssignableMapperOperator (Int32->Int32)
            +- PersonName: AssignableMapperOperator (String->String)
            +- DoB: AssignableMapperOperator (DateTime->DateTime)
", str);
    }

    [Fact]
    public void TestPrettyPrintDeferBuild()
    {
        CompanyWithDeferBuild[] companies = new CompanyWithDeferBuild[] {
        new CompanyWithDeferBuild {
            CompanyId=123,
            CompanyName="Acme Ltd",
            Employees = new Person[]{
                new Person { PersonId = 1, PersonName = "Wily C.", DoB=new DateTime(1990,01,01)},
                new Person { PersonId = 2, PersonName = "R.Runner", DoB=new DateTime(1991,07,01)},
            }
        },

        new CompanyWithDeferBuild {
            CompanyId=456,
            CompanyName="Globex Corporation",
            Employees = new Person[]{
                new Person { PersonId = 1, PersonName = "Homer", DoB=new DateTime(1990,01,01)},
                new Person { PersonId = 2, PersonName = "Marge", DoB=new DateTime(1991,07,01)},
            }
        } };

        var conf = new MapperConfiguration()
            .SetAutoRegisterTypes(true)
            .RegisterOperator<AssignableMapperOperator>()
            .RegisterOperator<ObjectSourceMapperOperator>()
            .RegisterOperator<MemberwiseMapperOperator>()
            .RegisterOperator<EnumerableMapperOperator>();
        var mapper = new ObjectMapper(conf);

        var op = mapper.GetMapperOperator(typeof(CompanyWithDeferBuild[]), typeof(List<CompanyDto>));
        var str = op.PrettyPrint();

        // without specifying actual source object, the operator plan stops at the ObjectSourceMapperOperator
        Assert.Equal(@"+- EnumerableMapperOperator (CompanyWithDeferBuild[]->List`1)
   +- []: MemberwiseMapperOperator (CompanyWithDeferBuild->CompanyDto)
      +- CompanyId: AssignableMapperOperator (Int32->Int32)
      +- CompanyName: AssignableMapperOperator (String->String)
      +- Employees: ObjectSourceMapperOperator (Object->List`1)
         +- *: <undefined>
", str);

        // Map an object using the operator - this will generate an exact plan
        var mapped = op.Map(companies);
        str = op.PrettyPrint(); // Get updated operator plan

        // The operator has mapped a source object - it is now able to show the
        // actual operator plan used.
        Assert.Equal(@"+- EnumerableMapperOperator (CompanyWithDeferBuild[]->List`1)
   +- []: MemberwiseMapperOperator (CompanyWithDeferBuild->CompanyDto)
      +- CompanyId: AssignableMapperOperator (Int32->Int32)
      +- CompanyName: AssignableMapperOperator (String->String)
      +- Employees: ObjectSourceMapperOperator (Object->List`1)
         +- *: EnumerableMapperOperator (Person[]->List`1)
            +- []: MemberwiseMapperOperator (Person->PersonDto)
               +- PersonId: AssignableMapperOperator (Int32->Int32)
               +- PersonName: AssignableMapperOperator (String->String)
               +- DoB: AssignableMapperOperator (DateTime->DateTime)
", str);
        Assert.IsType<List<CompanyDto>>(mapped);
        Assert.NotNull(mapped);
    }
}