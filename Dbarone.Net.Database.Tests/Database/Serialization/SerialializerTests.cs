namespace Dbarone.Net.Database.Tests;
using Xunit;

public class SerializerTests
{
    [Fact]
    public void Serializer_Serialize()
    {
        var entity = TestEntity.Create();
        for (int i = 0; i < 100000; i++)
        {
            var bytes = new EntitySerializer().Serialize(entity);
        }
    }
}