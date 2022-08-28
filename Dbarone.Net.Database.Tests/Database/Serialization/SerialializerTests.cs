namespace Dbarone.Net.Database.Tests;
using Xunit;
using Dbarone.Net.Extensions.Object;

public class SerializerTests
{
    [Fact]
    public void Serializer_Serialize()
    {
        var entity = TestEntity.Create();
        for (int i = 0; i < 1; i++)
        {
            var columns = Serializer.GetColumnInfo(typeof(TestEntity));
            var bytes = Serializer.Serialize(entity);
            var entity2 = Serializer.Deserialize<TestEntity>(bytes);
            
            // Check deserialised entity same as original entity.
            Assert.True(entity2.ValueEquals(entity));
        }
    }
}