namespace Dbarone.Net.Database.Tests;
using Xunit;
using Dbarone.Net.Database;
using System.IO;

public class Connection
{
    [Fact]
    public void CreateDatabase()
    {
        // Arrange
        var dbName = "mydb1.db";
        if (File.Exists(dbName))
        {
            File.Delete(dbName);
        }

        // Act
        var db = Database.Create(dbName);

        // Assert
        Assert.NotNull(db);
        Assert.IsAssignableFrom<IDatabase>(db);
        Assert.True(File.Exists(dbName));
    }

    [Fact]
    public void OpenDatabase()
    {
        // Arrange
        var dbName = "mydb2.db";
        if (File.Exists(dbName))
        {
            File.Delete(dbName);
        }

        // Act
        using (var db = Database.Create(dbName))
        {

        }

        // Should dispose db at this point (free file stream locks).
        using (var db = Database.Open(dbName, false))
        {
            // Assert
            Assert.NotNull(db);
            Assert.IsAssignableFrom<IDatabase>(db);
            Assert.True(File.Exists(dbName));
        }

    }
}