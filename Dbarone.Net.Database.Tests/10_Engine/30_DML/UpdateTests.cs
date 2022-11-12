namespace Dbarone.Net.Database.Tests;
using Xunit;
using Dbarone.Net.Database;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Text;


public class UpdateTests : TestBase
{
    [Theory]
    [InlineData(1000, 1000)]     // No testing of overflows
    [InlineData(1000, 10000)]    // include overflows
    public void Test_UpdateRaw(int iterations, int maxStringLength)
    {
        // Arrange
        var dbName = GetDatabaseFileNameFromMethod();
        TableInfo? table = null;
        using (var db = CreateDatabaseWithOverwriteIfExists(dbName))
        {
            table = CreateTableFromEntity<PersonInfo>(db);
            var people = PersonInfo.CreateTestData();
            db.BulkInsert(table.TableName, people);
            db.CheckPoint();    // Save pages to disk

            for (int i = 0; i < iterations; i++)
            {
                var random = new Random();
                var id = random.Next() % 10;
                // Length up to 10,000 will result in some rows using overflow storage.
                var randomLength = 1 + random.Next(maxStringLength);
                db.UpdateRaw(
                    table.TableName,
                    (dict) => { dict!["Comments"] = GetRandomString(randomLength); return dict; },
                    (dict) => dict!["PersonId"]!.Equals(id)
                );
            }
            db.CheckPoint();    // Save pages to disk            
        }

        using (var db = Engine.Open(dbName, false))
        {
            // Assert
            var people = db.Read<PersonInfo>(table.TableName);
            Assert.Equal(10, people.Count());

            // We only have 10 base records that are updated.
            // Approximate worst case is each record is in its own page, and is overflow
            // requiring 2 overflow pages (worst case scenario test is each row requiring
            // approx 10000 bytes storage), therefore 3 pages per record * 10 rows + 3
            // system pages (boot, table, column) This should be approximately true in all
            // scenarios of this test so long as maxStringLength <= 10000.
            Assert.True(db.Database().PageCount < 33);
        }
    }

    [Theory]
    [InlineData(1000, 1000)]     // No testing of overflows
    [InlineData(1000, 10000)]    // include overflows
    public void Test_Update(int iterations, int maxStringLength)
    {
        // Arrange
        var dbName = GetDatabaseFileNameFromMethod();
        TableInfo? table = null;
        using (var db = CreateDatabaseWithOverwriteIfExists(dbName))
        {
            table = CreateTableFromEntity<PersonInfo>(db);
            var people = PersonInfo.CreateTestData();
            db.BulkInsert(table.TableName, people);
            db.CheckPoint();    // Save pages to disk

            for (int i = 0; i < iterations; i++)
            {
                var random = new Random();
                var id = random.Next() % 10;
                // Length up to 10,000 will result in some rows using overflow storage.
                var randomLength = 1 + random.Next(maxStringLength);
                db.Update<PersonInfo>(
                    table.TableName,
                    (person) => { person.Comments = GetRandomString(randomLength); return person; },
                    (person) => person.PersonId.Equals(id)
                );
            }
            db.CheckPoint();    // Save pages to disk
        }

        using (var db = Engine.Open(dbName, false))
        {
            // Assert
            var people = db.Read<PersonInfo>(table.TableName);
            Assert.Equal(10, people.Count());

            // We only have 10 base records that are updated.
            // Approximate worst case is each record is in its own page, and is overflow
            // requiring 2 overflow pages (worst case scenario test is each row requiring
            // approx 10000 bytes storage), therefore 3 pages per record * 10 rows + 3
            // system pages (boot, table, column) This should be approximately true in all
            // scenarios of this test so long as maxStringLength <= 10000.
            Assert.True(db.Database().PageCount < 33);
        }
    }

    [Fact]
    public void Test_UpdateMultipleRowsAffected()
    {
        // Arrange
        var dbName = GetDatabaseFileNameFromMethod();
        TableInfo? table = null;
        int rowsAffected = 0;
        using (var db = CreateDatabaseWithOverwriteIfExists(dbName))
        {
            table = CreateTableFromEntity<PersonInfo>(db);
            var people = PersonInfo.CreateTestData();
            db.BulkInsert(table.TableName, people);

            /// This should update 4 rows
            rowsAffected = db.Update<PersonInfo>(
                table.TableName,
                (person) => { person.Comments = "XXX"; return person; },
                (person) => person.Comments == "ZZZ"
            );
            db.CheckPoint();    // Save pages to disk
        }

        using (var db = Engine.Open(dbName, false))
        {
            // Assert
            var people = db.Read<PersonInfo>(table.TableName).Where(p => p.Comments == "XXX");
            Assert.Equal(7, people.Count());    // original 3 + 4 ZZZ's replaced as XXX
            Assert.Equal(4, rowsAffected);
        }
    }

    [Fact]
    public void Test_UpdateZeroRowsAffected()
    {
        // Arrange
        var dbName = GetDatabaseFileNameFromMethod();
        TableInfo? table = null;
        int rowsAffected = 0;
        using (var db = CreateDatabaseWithOverwriteIfExists(dbName))
        {
            table = CreateTableFromEntity<PersonInfo>(db);
            var people = PersonInfo.CreateTestData();
            db.BulkInsert(table.TableName, people);

            /// This should update 4 rows
            rowsAffected = db.Update<PersonInfo>(
                table.TableName,
                (person) => { person.Comments = "XXX"; return person; },
                (person) => person.Comments == "AAA"
            );
            db.CheckPoint();    // Save pages to disk
        }

        using (var db = Engine.Open(dbName, false))
        {
            // Assert
            Assert.Equal(0, rowsAffected);
        }
    }
}