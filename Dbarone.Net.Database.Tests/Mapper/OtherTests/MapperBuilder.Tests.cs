using Xunit;
using System;
using Dbarone.Net.Database.Mapper;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;

namespace Dbarone.Net.Database.Tests.Mapper;

public class MapperBuilderTests
{
    [Theory]
    [InlineData(typeof(int), false)]
    [InlineData(typeof(ArrayList), true)]
    [InlineData(typeof(IEnumerable), true)]
    [InlineData(typeof(List<>), true)]
    [InlineData(typeof(List<int>), true)]
    public void TestBuild_IsEnumerablePropertySetCorrectly(Type type, bool expected)
    {
        MapperBuilder builder = new MapperBuilder(new MapperConfiguration()
            .RegisterType(type)
        );
        builder.Build();
        var buildType = builder.GetBuildTypeFor(type);
        Assert.Equal(expected, buildType.IsEnumerable);
    }

    [Theory]
    [InlineData(typeof(int), false)]
    [InlineData(typeof(ArrayList), false)]
    [InlineData(typeof(IEnumerable), false)]
    [InlineData(typeof(List<>), true)]
    [InlineData(typeof(List<int>), false)]
    public void TestBuild_IsOpenGenericPropertySetCorrectly(Type type, bool expected)
    {
        MapperBuilder builder = new MapperBuilder(new MapperConfiguration()
            .RegisterType(type)
        );
        builder.Build();
        var buildType = builder.GetBuildTypeFor(type);
        Assert.Equal(expected, buildType.isOpenGeneric);
    }

    [Theory]
    [InlineData(typeof(int), false)]
    [InlineData(typeof(int?), true)]
    public void TestBuild_IsNullablePropertySetCorrectly(Type type, bool expected)
    {
        MapperBuilder builder = new MapperBuilder(new MapperConfiguration()
            .RegisterType(type)
        );
        builder.Build();
        var buildType = builder.GetBuildTypeFor(type);
        Assert.Equal(expected, buildType.IsNullable);
    }

    [Theory]
    [InlineData(typeof(object), typeof(BuiltinMemberResolver))]
    [InlineData(typeof(ExpandoObject), typeof(DynamicMemberResolver))]
    public void Test_GetResolver(Type type, Type expectedResolverType)
    {
        MapperBuilder builder = new MapperBuilder(new MapperConfiguration().SetAutoRegisterTypes(true));
        var actualResolver = builder.GetResolver(type);
        Assert.Equal<Type>(expectedResolverType, actualResolver.GetType());
    }
}