namespace Dbarone.Net.Database.Tests;
using Xunit;
using Dbarone.Net.Database;
using System.IO;
using System.Linq;

public class CreateTableTests : TestBase
{
    class Customer
    {
        public string CustomerName { get; set; } = default!;
        public int CustomerId { get; set; }
    }

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
            db.CreateTable<Customer>(tableName);
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

    [Fact]
    public void TestCreate2TablesAndCheckColumns()
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
            ColumnInfo[] columns = {
                new ColumnInfo("AddressId", DataType.Int32, false),
                new ColumnInfo("Address1", DataType.String, false),
                new ColumnInfo("Address2", DataType.String, false),
                new ColumnInfo("Country", DataType.String, false)
            };

            db.CreateTable<Customer>(tableName);
            db.CreateTable("Addresses", columns);
            db.CheckPoint();    // Save pages to disk
        }

        // Assert
        using (var db = Engine.Open(dbName, false))
        {
            // Assert
            var tables = db.Tables();
            Assert.Equal(2, tables.Count());                    // 2 tables
            Assert.Equal(2, db.Columns("Customers").Count());   // Customers has 2 columns
            Assert.Equal(4, db.Columns("Addresses").Count());   // Addresses has 4 columns
            Assert.Equal(tableName, tables.First().TableName);
        }
    }
}