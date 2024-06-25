namespace Dbarone.Net.Database.Tests;
using Xunit;
using Dbarone.Net.Database;
using System.IO;
using System.Linq;
using Dbarone.Net.Document;

public class StaticMethodTests : TestBase
{
    [Fact]
    public void CreateDatabaseTest()
    {
        // Arrange
        var dbName = GetDatabaseFileNameFromMethod();
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
    public void CreateDatabaseTextEncodingLatin1()
    {
        // Arrange
        var dbName = GetDatabaseFileNameFromMethod();
        if (File.Exists(dbName))
        {
            File.Delete(dbName);
        }

        // Act
        using (var db = Engine.Create(dbName, new CreateDatabaseOptions { TextEncoding = TextEncoding.Latin1 }))
        {
            db.CheckPoint();
        }

        using (var db = Engine.Open(dbName, false))
        {
            // Assert
            Assert.NotNull(db);
            Assert.IsAssignableFrom<IEngine>(db);
            Assert.True(File.Exists(dbName));
            Assert.Equal(TextEncoding.Latin1, db.Database().TextEncoding);
        }
    }

    [Fact]
    public void OpenDatabase()
    {
        // Arrange
        var dbName = GetDatabaseFileNameFromMethod();
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
        var dbName = GetDatabaseFileNameFromMethod();
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