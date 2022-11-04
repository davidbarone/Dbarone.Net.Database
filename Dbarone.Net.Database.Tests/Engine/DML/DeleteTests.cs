namespace Dbarone.Net.Database.Tests;
using Xunit;
using Dbarone.Net.Database;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class DeleteTests : TestBase
{
    [Fact]
    public void TestDelete()
    {
        // Arrange
        var dbName = GetDatabaseFileNameFromMethod();
        if (File.Exists(dbName))
        {
            File.Delete(dbName);
        }
        var tableName = "Post";

        // Act
        using (var db = Engine.Create(dbName))
        {
            ColumnInfo[] columns = {
                new ColumnInfo("PostId", DataType.Int32, false),
                new ColumnInfo("PostTitle", DataType.String, false)
            };

            db.CreateTable(tableName, columns);
            db.Insert("Post", new Dictionary<string, object?>{
                {"PostId", 1},
                {"PostTitle", "First Post"}
            });
            db.Insert("Post", new Dictionary<string, object?>{
                {"PostId", 2},
                {"PostTitle", "Second Post"}
            });
            // Delete post #2
            db.DeleteRaw("Post", (row) => { return (int?)(row!["PostId"])==2 ? true : false; });
            db.CheckPoint();    // Save pages to disk
        }

        // Assert
        using (var db = Engine.Open(dbName, false))
        {
            // Assert
            var tables = db.Tables();
            Assert.Single(tables);                    // 1 table
            Assert.Equal(tableName, tables.First().TableName);
            Assert.Single(db.ReadRaw(tableName));
        }
    }
}