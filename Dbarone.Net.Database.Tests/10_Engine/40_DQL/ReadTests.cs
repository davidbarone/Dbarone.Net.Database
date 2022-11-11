namespace Dbarone.Net.Database.Tests;
using Xunit;
using Dbarone.Net.Database;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;
using Dbarone.Net.Extensions.Object;



public class ReadTests : TestBase
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TestReadEntity(bool tableHasRow)
    {
        var dbName = GetDatabaseFileNameFromMethod();
        TableInfo? table = null;
        var customer = new CustomerExInfo(123, DateTime.Today, "FooBarBaz", true);

        using (var db = CreateDatabaseWithOverwriteIfExists(dbName))
        {
            table = CreateTableFromEntity<CustomerExInfo>(db);
            if (tableHasRow)
            {
                db.Insert(table.TableName, customer);
            }
            db.CheckPoint();    // Save pages to disk
        }

        using (var db = Engine.Open(dbName, false))
        {
            var data = db.Read<CustomerExInfo>(table.TableName);
            
            if (tableHasRow) {
                Assert.True(data.First().ValueEquals(customer));
            } else {
                Assert.Empty(data);
            }
        }
    }
}