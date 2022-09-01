namespace Dbarone.Net.Database.Tests;
using Xunit;
using Dbarone.Net.Database;
using System.IO;

public class BootPageTest
{

    [Fact]
    public void TestCreateBootPage()
    {

        // Arrange
        // Arrange
        var dbName = "TestCreateBootPage.db";
        if (File.Exists(dbName))
        {
            File.Delete(dbName);
        }

        // Act
        using (var db = Database.Create(dbName))
        {
            db.CheckPoint();    // Save pages to disk
        }

        // Assert
        using (var db = Database.Open(dbName, false))
        {
            // Assert
            var dbInfo = db.GetDatabaseInfo();
            Assert.Equal(System.DateTime.Now.Date, dbInfo.CreationTime.Date);
            Assert.Equal(1, dbInfo.Version);
            Assert.Equal("Dbarone.Net.Database", dbInfo.Magic);
            Assert.Equal(3, dbInfo.PageCount);
        }
    }
}
