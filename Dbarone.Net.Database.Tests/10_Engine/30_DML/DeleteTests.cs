namespace Dbarone.Net.Database.Tests;
using Xunit;
using Dbarone.Net.Database;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class DeleteTests : TestBase
{
    [Theory]
    [InlineData("PersonId", 1, 1)]
    [InlineData("PersonId", -1, 0)]
    [InlineData("Comments", "XXX", 3)]
    public void Test_Delete(string columnName, object value, int expectedRowsAffected)
    {
        // Arrange
        var dbName = GetDatabaseFileNameFromMethod();
        int rowsAffected = 0;
        TableInfo? table = null;

        using (var db = CreateDatabaseWithOverwriteIfExists(dbName))
        {
            table = CreateTableFromEntity<PersonInfo>(db);
            var people = PersonInfo.CreateTestData();
            db.BulkInsert<PersonInfo>(table.TableName, people);
            rowsAffected = db.DeleteRaw(table.TableName, (row) => { return (row![columnName])!.Equals(value) ? true : false; });
            db.CheckPoint();    // Save pages to disk
        }

        using (var db = Engine.Open(dbName, false))
        {
            // Assert
            var tables = db.Tables();
            Assert.Single(tables);                    // 1 table
            Assert.Equal(expectedRowsAffected, rowsAffected);
            Assert.Equal(10 - expectedRowsAffected, db.ReadRaw(table.TableName).Count());
        }
    }
}