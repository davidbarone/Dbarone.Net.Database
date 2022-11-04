namespace Dbarone.Net.Database.Tests;
using Xunit;
using System;
using Dbarone.Net.Database;
using System.IO;
using System.Linq;
using System.Text;

public class OverflowPageTests : TestBase
{
    public class Comments {
        public int CommentId { get; set; }
        public string Comment { get; set; } = default!;
    }

    private string GetRandomString(int length) {
        StringBuilder sb = new StringBuilder();
        System.Random random = new Random();
        for (int i = 0; i < length; i++){
            sb.Append(((char)(random.Next(1, 26) + 64)).ToString());
        }
        return sb.ToString();
    }   

    [Fact]
    public void TestWritingLargeStringValue()
    {
        // Arrange
        var dbName = GetDatabaseFileNameFromMethod();
        if (File.Exists(dbName))
        {
            File.Delete(dbName);
        }
        var tableName = "Comments";
        var commentStr = GetRandomString(10000);

        // Act
        using (var db = Engine.Create(dbName))
        {
            db.CreateTable<Comments>(tableName);

            // Create 1 record with long comment
            Comments comment = new Comments();
            comment.CommentId = 1;
            comment.Comment = commentStr;
            db.Insert("Comments", comment);
            db.CheckPoint();    // Save pages to disk
        }


        // Assert
        using (var db = Engine.Open(dbName, false))
        {
            // Assert
            var comment = db.ReadRaw("Comments").First();
            Assert.Equal(commentStr, comment["Comment"]);
        }
    }
}
