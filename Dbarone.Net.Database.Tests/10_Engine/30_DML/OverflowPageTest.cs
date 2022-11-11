namespace Dbarone.Net.Database.Tests;
using Xunit;
using System;
using Dbarone.Net.Database;
using System.IO;
using System.Linq;
using System.Text;

public class OverflowPageTests : TestBase
{
    [Theory]
    //[InlineData(10000)]
    [InlineData(100000)]    // Goes over short limit
    [InlineData(1000000)]   // Goes over short limit
    public void TestWritingLargeStringValue(int stringLength)
    {
        var dbName = GetDatabaseFileNameFromMethod();
        TableInfo? table = null;
        var commentStr = GetRandomString(stringLength);

        using (var db = CreateDatabaseWithOverwriteIfExists(dbName))
        {
            table = CreateTableFromEntity<CommentInfo>(db);
            var comment = new CommentInfo(1, commentStr);
            db.Insert(table.TableName, comment);
            db.CheckPoint();    // Save pages to disk
        }

        using (var db = Engine.Open(dbName, false))
        {
            var comment = db.Read<CommentInfo>(table.TableName).First();
            Assert.Equal(commentStr, comment!.Comment);
        }
    }
}
