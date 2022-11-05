namespace Dbarone.Net.Database.Tests;
using Xunit;
using Dbarone.Net.Database;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class DataTests1 : TestBase
{
    [Fact]
    public void TestWriteSingleDataRow()
    {
        // Arrange
        var dbName = GetDatabaseFileNameFromMethod();
        if (File.Exists(dbName))
        {
            File.Delete(dbName);
        }
        var tableName = "Addresses";

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
            Assert.Single(tables);                    // 1 table
            Assert.Equal(tableName, tables.First().TableName);
        }
    }

    [Fact]
    public void TestWriteAndReadSingleDataRow()
    {

        // Arrange
        var dbName = GetDatabaseFileNameFromMethod();
        if (File.Exists(dbName))
        {
            File.Delete(dbName);
        }
        var tableName = "Addresses";

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
            Assert.Single(tables);                    // 1 table
            Assert.Equal(tableName, tables.First().TableName);

            // Read table
            var data = db.ReadRaw(tableName);
            Assert.Single(data);
            Assert.Equal(123, data.First()["AddressId"]);
            Assert.Equal("4 Acacia Drive", data.First()["Address1"]);
        }
    }

    [Fact]
    public void TestWriteAndRead1000Row()
    {
        // Arrange
        var dbName = GetDatabaseFileNameFromMethod();
        if (File.Exists(dbName))
        {
            File.Delete(dbName);
        }
        var tableName = "Addresses";

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
            
            List<IDictionary<string, object?>> data = new List<IDictionary<string, object?>>();
            for (int i = 0; i < 10000; i++)
            {
                data.Add(new Dictionary<string, object?>(){
                    {"AddressId", i},
                    {"Address1", "4 Acacia Drive"},
                    {"Address2", "Summertown"},
                    {"Country", "USA"}
                });
            }
            db.BulkInsert("Addresses", data);
            db.CheckPoint();    // Save pages to disk
        }

        // Assert
        using (var db = Engine.Open(dbName, false))
        {
            // Assert
            var tables = db.Tables();
            Assert.Single(tables);                    // 1 table
            Assert.Equal(tableName, tables.First().TableName);

            // Read table
            var data = db.ReadRaw(tableName);
            Assert.Equal(10000, data.Count());
            Assert.Equal(0, data.First()["AddressId"]);
            Assert.Equal("4 Acacia Drive", data.First()["Address1"]);
        }
    }
}