namespace Dbarone.Net.Database.Tests;
using Xunit;
using Dbarone.Net.Database;
using System.IO;
using System.Linq;

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
            Assert.Equal(2, dbInfo.PageCount);  // (0) Boot, (1) SystemTable
        }
    }

    class Customer
    {
        public string CustomerName { get; set; }
        public int CustomerId { get; set; }
    }

    [Fact]
    public void TestCreateTableFromEntity()
    {
        // Arrange
        var tableName = "Customers";
        var dbName = "mydb4.db";
        if (File.Exists(dbName))
        {
            File.Delete(dbName);
        }

        // Act
        using (var db = Database.Create(dbName))
        {
            db.CreateTable<Customer>(tableName);
            
            db.CheckPoint();    // Save pages to disk
        }

        // Assert
        using (var db = Database.Open(dbName, false))
        {
            // Assert
            var tables = db.Tables();
            Assert.Single(tables);
            Assert.Equal(tableName, tables.First().TableName);
        }
    }

    public void TestCreate2TablesAndCheckColumns() {

        // Arrange
        var tableName = "Customers";
        var dbName = "mydb5.db";
        if (File.Exists(dbName))
        {
            File.Delete(dbName);
        }

        // Act
        using (var db = Database.Create(dbName))
        {
            db.CreateTable<Customer>(tableName);
            db.CreateTable("Address", )
            
            db.CheckPoint();    // Save pages to disk
        }

    }
}