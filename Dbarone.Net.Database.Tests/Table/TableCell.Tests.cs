using System;
using Xunit;
namespace Dbarone.Net.Database;

public class TableCellTests
{

    [Fact]
    public void Test_TableCell_Ctor_Null()
    {
        TableCell cell = new TableCell();
        Assert.Null(cell.RawValue);
        Assert.Equal(DocumentType.Null, cell.Type);
    }

    [Fact]
    public void Test_TableCell_Ctor_Boolean()
    {
        bool boolValue = true;
        TableCell cell = boolValue;
        Assert.Equal(boolValue, cell.RawValue);
        Assert.Equal(DocumentType.Boolean, cell.Type);
    }

    [Fact]
    public void Test_TableCell_Ctor_Int()
    {
        // all integer types treated as signed long.
        ulong intValue = ulong.MaxValue;
        TableCell cell = intValue;
        Assert.Equal(intValue, cell.RawValue);
        Assert.Equal(DocumentType.Integer, cell.Type);
    }

    [Fact]
    public void Test_TableCell_Ctor_Real()
    {
        double doubleValue = double.MaxValue;
        TableCell cell = doubleValue;
        Assert.Equal(doubleValue, cell.RawValue);
        Assert.Equal(DocumentType.Real, cell.Type);
    }

    [Fact]
    public void Test_TableCell_Ctor_DateTime()
    {
        DateTime datetimeValue = DateTime.Now;
        TableCell cell = datetimeValue;
        Assert.Equal(datetimeValue, cell.RawValue);
        Assert.Equal(DocumentType.DateTime, cell.Type);
    }

    [Fact]
    public void Test_TableCell_Ctor_Text()
    {
        string stringValue = "foobarbaz";
        TableCell cell = stringValue;
        Assert.Equal(stringValue, cell.RawValue);
        Assert.Equal(DocumentType.Text, cell.Type);
    }

    [Fact]
    public void Test_TableCell_Ctor_Blob()
    {
        byte[] arrayValue = new byte[] { 100, 101, 102, 103, 104, 105 };
        TableCell cell = arrayValue;
        Assert.Equal(arrayValue, cell.RawValue);
        Assert.Equal(DocumentType.Blob, cell.Type);
    }
}