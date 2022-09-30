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
        var tableName = "Addresses";
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

            db.CreateTable(tableName, columns);
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
            Assert.Equal(1, tables.Count());                    // 1 table
            Assert.Equal(tableName, tables.First().TableName);
        }
    }
}