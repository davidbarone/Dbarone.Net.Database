using System;
using Dbarone.Net.Database;
using Xunit;

public class TableRowTests
{
    [Fact]
    public void ImplicitOperatorAndCellTypeTest()
    {
        TableRow row = new TableRow();
        row["number"] = 123;
        row["nullable_number"] = (int?)123;
        row["boolean"] = true;
        row["text"] = "foobar";
        row["real"] = 123.456;
        row["datetime"] = DateTime.Now;
        row["blob"] = new byte[] { 1, 2, 3, 4, 5 };

        Assert.Equal(DocumentType.Integer, row["number"].Type);
        Assert.Equal(DocumentType.Integer, row["nullable_number"].Type);
        Assert.Equal(DocumentType.Boolean, row["boolean"].Type);
        Assert.Equal(DocumentType.Text, row["text"].Type);
        Assert.Equal(DocumentType.Real, row["real"].Type);
        Assert.Equal(DocumentType.DateTime, row["datetime"].Type);
        Assert.Equal(DocumentType.Blob, row["blob"].Type);
    }
}