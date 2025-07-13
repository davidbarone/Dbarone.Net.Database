namespace Dbarone.Net.Mapper.Tests;
using Dbarone.Net.Mapper;
using Dbarone.Net.Extensions.Object;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using Xunit;
using Dbarone.Net.Database.Mapper;

public class BuiltinValueTypes
{
    public bool BoolValue { get; set; }
    public byte ByteValue { get; set; }
    public sbyte SByteValue { get; set; }
    public char CharValue { get; set; }
    public decimal DecimalValue { get; set; }
    public double DoubleValue { get; set; }
    public float FloatValue { get; set; }
    public int IntValue { get; set; }
    public uint UIntValue { get; set; }
    public nint NIntValue { get; set; }
    public nuint NUIntValue { get; set; }
    public long LongValue { get; set; }
    public ulong ULongValue { get; set; }
    public short ShortValue { get; set; }
    public ushort UShortValue { get; set; }

    public static BuiltinValueTypes CreateMin()
    {
        return new BuiltinValueTypes
        {
            BoolValue = false,
            ByteValue = byte.MinValue,
            SByteValue = sbyte.MinValue,
            CharValue = char.MinValue,
            DecimalValue = decimal.MinValue,
            DoubleValue = double.MinValue,
            FloatValue = float.MinValue,
            IntValue = int.MinValue,
            UIntValue = uint.MinValue,
            NIntValue = nint.MinValue,
            NUIntValue = nuint.MinValue,
            LongValue = long.MinValue,
            ULongValue = ulong.MinValue,
            ShortValue = short.MinValue,
            UShortValue = ushort.MinValue
        };
    }
    public static BuiltinValueTypes CreateMax()
    {
        return new BuiltinValueTypes
        {
            BoolValue = true,
            ByteValue = byte.MaxValue,
            SByteValue = sbyte.MaxValue,
            CharValue = char.MaxValue,
            DecimalValue = decimal.MaxValue,
            DoubleValue = double.MaxValue,
            FloatValue = float.MaxValue,
            IntValue = int.MaxValue,
            UIntValue = uint.MaxValue,
            NIntValue = nint.MaxValue,
            NUIntValue = nuint.MaxValue,
            LongValue = long.MaxValue,
            ULongValue = ulong.MaxValue,
            ShortValue = short.MaxValue,
            UShortValue = ushort.MaxValue
        };
    }
}

public class BuiltinValueTypesNullable
{
    public bool? BoolValue { get; set; }
    public byte? ByteValue { get; set; }
    public sbyte? SByteValue { get; set; }
    public char? CharValue { get; set; }
    public decimal? DecimalValue { get; set; }
    public double? DoubleValue { get; set; }
    public float? FloatValue { get; set; }
    public int? IntValue { get; set; }
    public uint? UIntValue { get; set; }
    public nint? NIntValue { get; set; }
    public nuint? NUIntValue { get; set; }
    public long? LongValue { get; set; }
    public ulong? ULongValue { get; set; }
    public short? ShortValue { get; set; }
    public ushort? UShortValue { get; set; }

    public static BuiltinValueTypesNullable CreateMin()
    {
        return new BuiltinValueTypesNullable
        {
            BoolValue = false,
            ByteValue = byte.MinValue,
            SByteValue = sbyte.MinValue,
            CharValue = char.MinValue,
            DecimalValue = decimal.MinValue,
            DoubleValue = double.MinValue,
            FloatValue = float.MinValue,
            IntValue = int.MinValue,
            UIntValue = uint.MinValue,
            NIntValue = nint.MinValue,
            NUIntValue = nuint.MinValue,
            LongValue = long.MinValue,
            ULongValue = ulong.MinValue,
            ShortValue = short.MinValue,
            UShortValue = ushort.MinValue
        };
    }

    public static BuiltinValueTypesNullable CreateMax()
    {
        return new BuiltinValueTypesNullable
        {
            BoolValue = true,
            ByteValue = byte.MaxValue,
            SByteValue = sbyte.MaxValue,
            CharValue = char.MaxValue,
            DecimalValue = decimal.MaxValue,
            DoubleValue = double.MaxValue,
            FloatValue = float.MaxValue,
            IntValue = int.MaxValue,
            UIntValue = uint.MaxValue,
            NIntValue = nint.MaxValue,
            NUIntValue = nuint.MaxValue,
            LongValue = long.MaxValue,
            ULongValue = ulong.MaxValue,
            ShortValue = short.MaxValue,
            UShortValue = ushort.MaxValue
        };
    }

    public static BuiltinValueTypesNullable CreateNull()
    {
        return new BuiltinValueTypesNullable
        {
            BoolValue = null,
            ByteValue = null,
            SByteValue = null,
            CharValue = null,
            DecimalValue = null,
            DoubleValue = null,
            FloatValue = null,
            IntValue = null,
            UIntValue = null,
            NIntValue = null,
            NUIntValue = null,
            LongValue = null,
            ULongValue = null,
            ShortValue = null,
            UShortValue = null
        };
    }
}

public class MapClassWithBuiltinTypesTests
{
    [Fact]
    public void Should_Map_Builtin_Value_Types_Min()
    {
        BuiltinValueTypes obj1 = BuiltinValueTypes.CreateMin();

        var conf = new MapperConfiguration()
            .RegisterType<BuiltinValueTypes>()
            .RegisterOperator<AssignableMapperOperator>();

        var mapper = new ObjectMapper(conf);

        var obj2 = mapper.Map<BuiltinValueTypes, BuiltinValueTypes>(obj1);
        Assert.True(obj1.ValueEquals(obj2));
    }

    [Fact]
    public void Should_Map_Builtin_Value_Types_Max()
    {
        BuiltinValueTypes obj1 = BuiltinValueTypes.CreateMin();

        var conf = new MapperConfiguration()
            .RegisterType<BuiltinValueTypes>()
            .RegisterOperator<AssignableMapperOperator>();

        var mapper = new ObjectMapper(conf);

        var obj2 = mapper.Map<BuiltinValueTypes, BuiltinValueTypes>(obj1);
        Assert.True(obj1.ValueEquals(obj2));
    }

    [Fact]
    public void Should_Map_Builtin_Value_Types_Nullable()
    {
        BuiltinValueTypesNullable obj1 = BuiltinValueTypesNullable.CreateNull();

        var conf = new MapperConfiguration()
           .RegisterType<BuiltinValueTypesNullable>()
           .RegisterOperator<NullableSourceMapperOperator>()
           .RegisterOperator<AssignableMapperOperator>()
           .RegisterOperator<MemberwiseMapperOperator>();

        var mapper = new ObjectMapper(conf);

        var obj2 = mapper.Map<BuiltinValueTypesNullable, BuiltinValueTypesNullable>(obj1);
        Assert.True(obj1.ValueEquals(obj2));
    }
}