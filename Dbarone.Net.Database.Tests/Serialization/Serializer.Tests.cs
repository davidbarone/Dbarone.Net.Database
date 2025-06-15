using System;
using Dbarone.Net.Database;
using Xunit;

public class SerializerTests()
{
    [Fact]
    public void BootDataToBytes()
    {
        try
        {
            BootData bd = new BootData();
            Serializer s = new Serializer(512, TextEncoding.UTF8);
            var bytes = s.Serialize(bd);
            Assert.True(bytes.Length > 0);
        }
        catch (Exception ex)
        {
            var a = ex;
        }
    }

    [Fact]
    public void DeserializeTest()
    {
        BootData bd1 = new BootData();
        bd1.Magic = "xxx";
        bd1.PageSize = 1234;
        Serializer s = new Serializer(512, TextEncoding.UTF8);
        var bytes = s.Serialize(bd1);

        // Deserialize
        BootData bd2 = (BootData)s.Deserialize(bytes, typeof(BootData));

        Assert.Equal("xxx", bd2.Magic);
        Assert.Equal(1234, bd2.PageSize);
    }
}