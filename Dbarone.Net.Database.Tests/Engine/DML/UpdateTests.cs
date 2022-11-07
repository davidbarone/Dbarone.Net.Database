namespace Dbarone.Net.Database.Tests;
using Xunit;
using Dbarone.Net.Database;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Text;

public class Person
{
    public int PersonId { get; set; }
    public string PersonName { get; set; } = default!;
    public string Comments { get; set; } = default!;
    public DateTime DoB { get; set; }

    public Person(int personId, string personName, string comments, DateTime dob)
    {
        this.PersonId = personId;
        this.PersonName = personName;
        this.Comments = comments;
        this.DoB = dob;
    }

    public Person() { }
}

public class UpdateTests : TestBase
{
    private string GetRandomString(int length)
    {
        StringBuilder sb = new StringBuilder();
        System.Random random = new Random();
        for (int i = 0; i < length; i++)
        {
            sb.Append(((char)(random.Next(1, 26) + 64)).ToString());
        }
        return sb.ToString();
    }

    [Theory]
    [InlineData(1000, 100)]     // No testing of overflows
    [InlineData(1000, 1000)]    // include overflows
    public void TestMultipleUpdates(int iterations, int maxStringLength)
    {
        // Arrange
        var dbName = GetDatabaseFileNameFromMethod();
        if (File.Exists(dbName))
        {
            File.Delete(dbName);
        }
        var tableName = "People";

        // Act
        using (var db = Engine.Create(dbName))
        {
            db.CreateTable<Person>(tableName);
            var people = new Person[] {
                new Person(0, "Fred", "XXX", DateTime.Today),
                new Person(1, "John", "XXX", DateTime.Today),
                new Person(2, "Tony", "XXX", DateTime.Today),
                new Person(3, "Ian", "XXX", DateTime.Today),
                new Person(4, "Paul", "XXX", DateTime.Today),
                new Person(5, "Stuart", "XXX", DateTime.Today),
                new Person(6, "Colin", "XXX", DateTime.Today),
                new Person(7, "Malcolm", "XXX", DateTime.Today),
                new Person(8, "David", "XXX", DateTime.Today),
                new Person(9, "Mark", "XXX", DateTime.Today)
            };
            db.BulkInsert(tableName, people);
            db.CheckPoint();    // Save pages to disk

            // Perform 1000 iterations, updating the records randomly
            for (int i = 0; i < iterations; i++)
            {
                var random = new Random();
                var id = random.Next() % 10;
                // Length up to 10,000 will result in some rows using overflow storage.
                var randomLength = 1 + random.Next(maxStringLength);
                db.UpdateRaw(
                    tableName,
                    (dict) => { dict!["Comments"] = GetRandomString(randomLength); return dict; },
                    (dict) => dict!["PersonId"]!.Equals(id)
                );
                db.CheckPoint(); // Must execute checkpoint after each update
            }
        }

        // Assert
        using (var db = Engine.Open(dbName, false))
        {
            // Assert
            var people = db.Read<Person>(tableName);
            Assert.Equal(10, people.Count());
            //Assert.Equal(5, db.Database().PageCount);
        }
    }
}