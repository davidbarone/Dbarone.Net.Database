namespace Dbarone.Net.Database.Tests;
using Xunit;
using Dbarone.Net.Database;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class DML_InsertTests : TestBase
{
    [Fact]
    public void Test_InsertRaw()
    {
        // Arrange
        var dbName = GetDatabaseFileNameFromMethod();
        TableInfo? table = null;
        int rowsAffected = 0;
        using (var db = CreateDatabaseWithOverwriteIfExists(dbName))
        {
            table = CreateTable(db, typeof(AddressInfo));
            rowsAffected = db.InsertRaw(table.TableName, new Dictionary<string, object?>{
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
            Assert.Equal(table.TableName, tables.First().TableName);
            Assert.Equal(1, rowsAffected);
        }
    }

    [Fact]
    public void Test_Insert()
    {
        // Arrange
        var dbName = GetDatabaseFileNameFromMethod();
        TableInfo? table = null;
        int rowsAffected = 0;
        using (var db = CreateDatabaseWithOverwriteIfExists(dbName))
        {
            table = CreateTable(db, typeof(AddressInfo));
            AddressInfo address = new AddressInfo();
            address.AddressId = 123;
            address.Address1 = "4 Acacia Drive";
            address.Address2 = "Summertown";
            address.Country = "USA";
            rowsAffected = db.Insert(table.TableName, address);
            db.CheckPoint();    // Save pages to disk
        }

        // Assert
        using (var db = Engine.Open(dbName, false))
        {
            // Assert
            var tables = db.Tables();
            Assert.Single(tables);
            Assert.Equal(table.TableName, tables.First().TableName);
            Assert.Equal(1, rowsAffected);
        }
    }

    [Fact]
    public void Test_InsertNull()
    {
        // Arrange
        var dbName = GetDatabaseFileNameFromMethod();
        TableInfo? table = null;
        int rowsAffected = 0;
        using (var db = CreateDatabaseWithOverwriteIfExists(dbName))
        {
            table = CreateTable(db, typeof(AddressInfo));
            AddressInfo? address = null;
            Assert.Throws<Dbarone.Net.Assertions.AssertionException>(() =>
            {
                rowsAffected = db.Insert(table.TableName, address); // should raise exception
            });
            db.CheckPoint();    // Save pages to disk
        }
    }

    [Fact]
    public void Test_WriteAndReadSingleDataRow()
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

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(10000)]
    public void Test_BulkInsertRaw(int numberOfRows)
    {
        // Arrange
        var dbName = GetDatabaseFileNameFromMethod();
        TableInfo? table = null;
        int rowsAffected = 0;

        // Act
        using (var db = CreateDatabaseWithOverwriteIfExists(dbName))
        {
            table = CreateTable(db, typeof(AddressInfo));

            List<IDictionary<string, object?>> data = new List<IDictionary<string, object?>>();
            for (int i = 0; i < numberOfRows; i++)
            {
                data.Add(new Dictionary<string, object?>(){
                    {"AddressId", i},
                    {"Address1", "4 Acacia Drive"},
                    {"Address2", "Summertown"},
                    {"Country", "USA"}
                });
            }
            rowsAffected = db.BulkInsert(table.TableName, data);
            db.CheckPoint();    // Save pages to disk
        }

        // Assert
        using (var db = Engine.Open(dbName, false))
        {
            // Assert
            Assert.Equal(numberOfRows, rowsAffected);

            var tables = db.Tables();
            Assert.Single(tables);                    // 1 table
            Assert.Equal(table.TableName, tables.First().TableName);

            // Read table
            var data = db.ReadRaw(table.TableName);
            if (data.Any())
            {
                Assert.Equal(numberOfRows, data.Count());
                Assert.Equal("USA", data.First()["Country"]);
                Assert.Equal("4 Acacia Drive", data.First()["Address1"]);
            }
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(10000)]
    public void Test_BulkInsert(int numberOfRows)
    {
        // Arrange
        var dbName = GetDatabaseFileNameFromMethod();
        TableInfo? table = null;
        int rowsAffected = 0;

        // Act
        using (var db = CreateDatabaseWithOverwriteIfExists(dbName))
        {
            table = CreateTableFromEntity<AddressInfo>(db);

            List<AddressInfo> data = new List<AddressInfo>();
            for (int i = 0; i < numberOfRows; i++)
            {
                AddressInfo address = new AddressInfo();
                address.AddressId = i;
                address.Address1 = "4 Acacia Drive";
                address.Address2 = "Summertown";
                address.Country = "USA";
                data.Add(address);
            }
            rowsAffected = db.BulkInsert(table.TableName, data);
            db.CheckPoint();    // Save pages to disk
        }

        // Assert
        using (var db = Engine.Open(dbName, false))
        {
            // Assert
            Assert.Equal(numberOfRows, rowsAffected);

            var tables = db.Tables();
            Assert.Single(tables);                    // 1 table
            Assert.Equal(table.TableName, tables.First().TableName);

            // Read table
            var data = db.Read<AddressInfo>(table.TableName);
            if (data.Any())
            {
                Assert.Equal(numberOfRows, data.Count());
                Assert.Equal("USA", data.First()!.Country);
                Assert.Equal("4 Acacia Drive", data.First()!.Address1);
            }
        }
    }
}