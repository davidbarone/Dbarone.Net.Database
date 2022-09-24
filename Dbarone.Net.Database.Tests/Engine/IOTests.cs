namespace Dbarone.Net.Database.Tests;
using Xunit;
using Dbarone.Net.Database;
using System.IO;
using System.Linq;

public class IOTests
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
        var db = Engine.Create(dbName);

        // Assert
        Assert.NotNull(db);
        Assert.IsAssignableFrom<IEngine>(db);
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
        using (var db = Engine.Create(dbName))
        {

        }

        // Should dispose db at this point (free file stream locks).
        using (var db = Engine.Open(dbName, false))
        {
            // Assert
            Assert.NotNull(db);
            Assert.IsAssignableFrom<IEngine>(db);
            Assert.True(File.Exists(dbName));
        }

    }

    [Fact]
    public void WhenCreateDatabaseCheckPagesCreated()
    {
        // Arrange
        var dbName = "mydb3.db";
        if (File.Exists(dbName))
        {
            File.Delete(dbName);
        }

        // Act
        using (var db = Engine.Create(dbName))
        {
            db.CheckPoint();    // Save pages to disk
        }

        // Assert
        using (var db = Engine.Open(dbName, false))
        {
            // Assert
            var dbInfo = db.Database();
            Assert.Equal(System.DateTime.Now.Date, dbInfo.CreationTime.Date);
            Assert.Equal(1, dbInfo.Version);
            Assert.Equal("Dbarone.Net.Database", dbInfo.Magic);
            Assert.Equal(2, dbInfo.PageCount);  // (0) Boot, (1) SystemTable
        }
    }
}