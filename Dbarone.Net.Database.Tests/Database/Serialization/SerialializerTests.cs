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
            var serializer = new EntitySerializer();
            var columns = serializer.GetColumnInfo(typeof(TestEntity));
            var bytes = serializer.Serialize(entity);
            var entity2 = serializer.Deserialize<TestEntity>(columns, bytes);
        }
    }
}