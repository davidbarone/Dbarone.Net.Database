using System;
using Dbarone.Net.Extensions;
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

    #region Implicit Operators

    [Fact]
    public void Test_TableCell_Implicit_Operator_Bool()
    {
        TableCell cell = true;
        Assert.Equal(true, cell.RawValue);
        Assert.Equal(DocumentType.Boolean, cell.Type);
    }

    [Fact]
    public void Test_TableCell_Implicit_Operator_Int()
    {
        // all ints stored as long
        TableCell cell = (short)123;
        Assert.Equal((long)123, cell.RawValue);
        Assert.Equal(DocumentType.Integer, cell.Type);
    }

    [Fact]
    public void Test_TableCell_Implicit_Operator_Real()
    {
        // all reals stored as double
        TableCell cell = (Single)123.45;
        Assert.Equal((double)123.45, cell.RawValue);
        Assert.Equal(DocumentType.Real, cell.Type);
    }

    [Fact]
    public void Test_TableCell_Implicit_Operator_DateTime()
    {
        DateTime now = DateTime.Now;
        TableCell cell = now;
        Assert.Equal(now, cell.RawValue);
        Assert.Equal(DocumentType.DateTime, cell.Type);
    }

    [Fact]
    public void Test_TableCell_Implicit_Operator_Text()
    {
        var foo = "foobarbaz";
        TableCell cell = foo;
        Assert.Equal(foo, cell.RawValue);
        Assert.Equal(DocumentType.Text, cell.Type);
    }

    [Fact]
    public void Test_TableCell_Implicit_Operator_Blob()
    {
        var blob = new byte[] { 1, 2, 3, 4, 5 };
        TableCell cell = blob;
        Assert.Equal(blob, cell.RawValue);
        Assert.Equal(DocumentType.Blob, cell.Type);
    }

    #endregion
}