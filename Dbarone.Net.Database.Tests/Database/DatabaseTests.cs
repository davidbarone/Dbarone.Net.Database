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
        using (var db = Database.Create(dbName))
        {
            db.CheckPoint();    // Save pages to disk
        }

        // Assert
        using (var db = Database.Open(dbName, false))
        {
            // Assert
            var dbInfo = db.GetDatabaseInfo();
            Assert.Equal(System.DateTime.Now.Date, dbInfo.CreationTime.Date);
            Assert.Equal(1, dbInfo.Version);
            Assert.Equal("Dbarone.Net.Database", dbInfo.Magic);
            Assert.Equal(3, dbInfo.PageCount);  // (0) Boot, (1) SystemTable, (2) SystemColumn
        }
    }

    [Fact]
    public void TestCreateTable() {

        // Arrange
        var dbName = "mydb4.db";
        if (File.Exists(dbName))
        {
            File.Delete(dbName);
        }

        // Act
        using (var db = Database.Create(dbName))
        {
            db.CreateTable<object>("Customers");
            db.CheckPoint();    // Save pages to disk
        }

        // Assert
        using (var db = Database.Open(dbName, false))
        {
            // Assert
            var dbInfo = db.GetDatabaseInfo();
            Assert.Equal(System.DateTime.Now.Date, dbInfo.CreationTime.Date);
            Assert.Equal(1, dbInfo.Version);
            Assert.Equal("Dbarone.Net.Database", dbInfo.Magic);
            Assert.Equal(3, dbInfo.PageCount);  // (0) Boot, (1) SystemTable, (2) SystemColumn
        }

    }
}