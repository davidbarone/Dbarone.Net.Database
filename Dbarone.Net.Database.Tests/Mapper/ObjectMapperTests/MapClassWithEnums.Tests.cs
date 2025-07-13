namespace Dbarone.Net.Mapper.Tests;
using Dbarone.Net.Mapper;
using Dbarone.Net.Extensions.Object;
using System.Linq.Expressions;
using Xunit;
using Dbarone.Net.Database.Mapper;

public class ClassWithDummyEnum
{
    public DummyEnum DummyEnum { get; set; }
}

public class ClassWithDummyInt
{
    public int DummyEnum { get; set; }
}

public class ClassWithDummyString
{
    public string DummyEnum { get; set; }
}

public enum DummyEnum : int
{
    A = 1,
    B = 2,
    C = 3
}

public class EnumType
{
    public DummyEnum EnumValue { get; set; } = DummyEnum.A;

    public static EnumType CreateMin()
    {
        return new EnumType
        {
            EnumValue = DummyEnum.A
        };
    }

    public static EnumType CreateMax()
    {
        return new EnumType
        {
            EnumValue = DummyEnum.C
        };
    }
}

public class EnumTypeNullable
{
    public DummyEnum? EnumValue { get; set; } = null;

    public static EnumTypeNullable CreateMin()
    {
        return new EnumTypeNullable
        {
            EnumValue = DummyEnum.A
        };
    }

    public static EnumTypeNullable CreateMax()
    {
        return new EnumTypeNullable
        {
            EnumValue = DummyEnum.C
        };
    }

    public static EnumTypeNullable CreateNull()
    {
        return new EnumTypeNullable
        {
            EnumValue = null
        };
    }
}

public class MapClassWithEnumsTests
{
    [Fact]
    public void Should_Map_Enum_Types()
    {
        EnumType obj1 = EnumType.CreateMax();
        var conf = new MapperConfiguration()
            .RegisterType<EnumType>()
            .RegisterOperator<AssignableMapperOperator>();
        var mapper = new ObjectMapper(conf);
        var obj2 = mapper.Map<EnumType, EnumType>(obj1);
        Assert.True(obj1.ValueEquals(obj2));
    }

    [Fact]
    public void Should_Map_Enum_Types_Nullable()
    {
        EnumTypeNullable obj1 = EnumTypeNullable.CreateMax();
        var conf = new MapperConfiguration()
            .RegisterType<EnumTypeNullable>()
            .RegisterOperator<AssignableMapperOperator>();
        var mapper = new ObjectMapper(conf);
        var obj2 = mapper.Map<EnumTypeNullable, EnumTypeNullable>(obj1);
        Assert.True(obj1.ValueEquals(obj2));
    }

    [Fact]
    public void Should_Map_Enum_Types_Nullable_Max()
    {
        EnumTypeNullable obj1 = EnumTypeNullable.CreateMax();
        var conf = new MapperConfiguration()
            .RegisterType<EnumTypeNullable>()
            .RegisterOperator<AssignableMapperOperator>();
        var mapper = new ObjectMapper(conf);
        var obj2 = mapper.Map<EnumTypeNullable, EnumTypeNullable>(obj1);
        Assert.True(obj1.ValueEquals(obj2));
    }

    [Fact]
    public void Test_Class_With_Enum_Value_Mapping()
    {
        ClassWithDummyEnum a = new ClassWithDummyEnum();
        a.DummyEnum = DummyEnum.C;

        var conf = new MapperConfiguration()
            .SetAutoRegisterTypes(true)
            .RegisterOperator<EnumSourceValueMapperOperator>()
            .RegisterOperator<EnumTargetValueMapperOperator>()
            .RegisterOperator<AssignableMapperOperator>()
            .RegisterOperator<MemberwiseMapperOperator>();
        var mapper = new ObjectMapper(conf);

        // Map
        var b = mapper.Map<ClassWithDummyEnum, ClassWithDummyInt>(a);
        Assert.Equal(3, b.DummyEnum);

        // Map back again
        var c = mapper.Map<ClassWithDummyInt, ClassWithDummyEnum>(b);
        Assert.Equal(DummyEnum.C, c.DummyEnum);
    }

    [Fact]
    public void Test_Class_With_Enum_String_Mapping()
    {
        ClassWithDummyEnum a = new ClassWithDummyEnum();
        a.DummyEnum = DummyEnum.C;

        var conf = new MapperConfiguration()
            .SetAutoRegisterTypes(true)
            .RegisterOperator<EnumSourceStringMapperOperator>()
            .RegisterOperator<EnumTargetStringMapperOperator>()
            .RegisterOperator<AssignableMapperOperator>()
            .RegisterOperator<MemberwiseMapperOperator>();
        var mapper = new ObjectMapper(conf);

        // Map
        var b = mapper.Map<ClassWithDummyEnum, ClassWithDummyString>(a);
        Assert.Equal("C", b.DummyEnum);

        // Map back again
        var c = mapper.Map<ClassWithDummyString, ClassWithDummyEnum>(b);
        Assert.Equal(DummyEnum.C, c.DummyEnum);
    }
}