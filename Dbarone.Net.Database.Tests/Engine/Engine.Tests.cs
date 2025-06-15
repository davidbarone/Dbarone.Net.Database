using Xunit;

namespace Dbarone.Net.Database.Tests;

public class EngineTests
{

    [Fact]
    public void CreateDatabase()
    {
        var engine = Engine.Create(new CreateDatabaseOptions
        {
            PageSize = 11,
            TextEncoding = TextEncoding.UTF8
        });
        Assert.NotNull(engine);
    }
}