namespace Dbarone.Net.Database.Tests;
using Xunit;
using Dbarone.Net.Database;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;
using Dbarone.Net.Extensions.Object;

public class Customer
{
    public int CustomerId { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string CustomerName { get; set; } = default!;
    public bool IsActive { get; set; }
    public Customer(int customerId, DateTime dateOfBirth, string customerName, bool isActive)
    {
        this.CustomerId = customerId;
        this.DateOfBirth = dateOfBirth;
        this.CustomerName = customerName;
        this.IsActive = isActive;
    }
    public Customer()
    {

    }
}

public class ReadTests : TestBase
{
    [Fact]
    public void TestReadEntity()
    {
        var dbName = GetDatabaseFileNameFromMethod();

        // Arrange
        if (File.Exists(dbName))
        {
            File.Delete(dbName);
        }
        var tableName = "Customers";
        var customer = new Customer(123, DateTime.Today, "FooBarBaz", true);

        using (var db = Engine.Create(dbName))
        {
            db.CreateTable<Customer>(tableName);
            db.Insert(tableName, customer);
            db.CheckPoint();    // Save pages to disk
        }

        using (var db = Engine.Open(dbName, false))
        {
            // Act
            var data = db.Read<Customer>(tableName).First();
            Assert.True(data.ValueEquals(customer));
        }
    }
}