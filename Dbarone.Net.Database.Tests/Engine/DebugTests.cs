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
       0             Boot                                   0        0    False        0
       1      SystemTable                                   1        0    False       43
       2     SystemColumn                                   3        0    False       87
       3             Data                          0        0        0    False        0
", str);
        }
    }

    [Fact]
    public void TestDebugPage()
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
            var str = db.DebugPage(2);
            Assert.Equal(@"PageType: SystemColumn
IsDirty: False
Headers.PageId: 2
Headers.PrevPageId: 
Headers.NextPageId: 
Headers.ParentObjectId: 
Headers.SlotsUsed: 3
Headers.TransactionId: 0
Headers.FreeOffset: 87

Slot #0: Offset: 0, Status Flags: [   ]
Slot #1: Offset: 29, Status Flags: [   ]
Slot #2: Offset: 58, Status Flags: [   ]
", str);
        }
    }
}




