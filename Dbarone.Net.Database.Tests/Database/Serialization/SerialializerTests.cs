namespace Dbarone.Net.Database.Tests;
using Xunit;

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
        }
    }
}