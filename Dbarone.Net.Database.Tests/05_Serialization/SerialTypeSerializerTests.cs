namespace Dbarone.Net.Database.Tests;
using Xunit;
using Dbarone.Net.Extensions.Object;
using DBAssert = Dbarone.Net.Assertions;

public class SerialTypeSerializerTests
{
    public ISerializer Serializer = new SerialTypeSerializer();
    
    [Fact]
    public void SerializerEx_Serialize()
    {
        var entity = TestEntity.Create();
        for (int i = 0; i < 1; i++)
        {
            var columns = Serializer.GetColumnsForType(typeof(TestEntity));
            var bytes = Serializer.Serialize(entity, RowStatus.None);
            var entity2 = Serializer.Deserialize<TestEntity>(bytes).Result;

            // Check deserialised entity same as original entity.
            Assert.True(entity2.ValueEquals(entity));
        }
    }

    [Fact]
    public void SerializerEx_CompletesIn()
    {
        // Serialise / deserialise 50,000 objects in 10 seconds.
        DBAssert.Assert.CompletesIn(() =>
        {
            var entity = TestEntity.Create();
            for (int i = 0; i < 50000; i++)
            {
                var columns = Serializer.GetColumnsForType(typeof(TestEntity));
                var bytes = Serializer.Serialize(entity, RowStatus.None);
                var entity2 = Serializer.Deserialize<TestEntity>(bytes).Result;
            }
        }, new System.TimeSpan(0, 0, 10));
    }
}