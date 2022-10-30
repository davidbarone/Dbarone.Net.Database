namespace Dbarone.Net.Database.Tests;
using Xunit;
using Dbarone.Net.Database;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class DebugTests : TestBase
{
    [Fact]
    public void TestDebugPages()
    {
        // Arrange
        var dbName = GetDatabaseFileNameFromMethod();
        if (File.Exists(dbName))
        {
            File.Delete(dbName);
        }
        var tableName = "Table1";

        // Act
        using (var db = Engine.Create(dbName))
        {
            ColumnInfo[] columns = {
                new ColumnInfo("Col1", DataType.Int32, false),
                new ColumnInfo("Col2", DataType.String, false),
                new ColumnInfo("Col3", DataType.String, false),
            };

            db.CreateTable(tableName, columns);
            db.CheckPoint();    // Save pages to disk
        }

        // Assert
        using (var db = Engine.Open(dbName, false))
        {
            // Assert
            var str = db.DebugPages();
            Assert.Equal(@"  PageId         PageType     Prev     Next   Parent    Slots     Tran    Dirty     Free
  ------         --------     ----     ----   ------    -----     ----    -----     ----
       0             Boot                                   0        0     True        0
       1      SystemTable                                   1        0     True       43
       2     SystemColumn                                   3        0     True       87
       3             Data                          0        0        0     True        0
", str);
        }
    }
}




