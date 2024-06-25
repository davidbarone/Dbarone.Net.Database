namespace Dbarone.Net.Database.Tests;
using Xunit;
using Dbarone.Net.Database;
using System.IO;
using System.Linq;

public class CreateTableTests : TestBase
{
    [Fact]
    public void TestCreateTableFromEntity()
    {
        // Arrange
        var dbName = GetDatabaseFileNameFromMethod();
        if (File.Exists(dbName))
        {
            File.Delete(dbName);
        }
        var tableName = "Customers";

        // Act
        using (var db = Engine.Create(dbName))
        {
            db.CreateTable(tableName);
            db.CheckPoint();    // Save pages to disk
        }

        // Assert
        using (var db = Engine.Open(dbName, false))
        {
            // Assert
            var tables = db.Tables();
            Assert.Single(tables);
            Assert.Equal(tableName, tables.First().TableName);
        }
    }
}