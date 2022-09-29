namespace Dbarone.Net.Database.Tests;
using Xunit;
using Dbarone.Net.Database;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class DMLTests
{
    [Fact]
    public void TestCreate2TablesAndCheckColumns()
    {

        // Arrange
        var tableName = "Customers";
        var dbName = "mydb6.db";
        if (File.Exists(dbName))
        {
            File.Delete(dbName);
        }

        // Act
        using (var db = Engine.Create(dbName))
        {
            ColumnInfo[] columns = {
                new ColumnInfo("AddressId", DataType.Int32, false),
                new ColumnInfo("Address1", DataType.String, false),
                new ColumnInfo("Address2", DataType.String, false),
                new ColumnInfo("Country", DataType.String, false)
            };

            db.CreateTable("Addresses", columns);
            db.Insert("Addresses", new Dictionary<string, object?>{
                {"AddressId", 123},
                {"Address1", "4 Acacia Drive"},
                {"Address2", "Summertown"},
                {"Country", "USA"}
            });
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