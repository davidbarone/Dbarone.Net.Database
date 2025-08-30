using System;
using Dbarone.Net.Database;
using Xunit;

public class PageTests
{
    private TableRow GetTableRow()
    {
        TableRow r = new TableRow();
        r["DUMMY"] = "x";
        return r;
    }

    [Fact]
    public void TestHeadersGetSet()
    {
        TableSerializer ser = new TableSerializer();
        var page = new Page(ser, 123);
        page.TableCount = 2;
        page.NextPageId = 456;
        page.PageType = PageType.Empty;

        Assert.Equal(2, page.TableCount);
        Assert.Equal(123, page.PageId);
        Assert.Equal(456, page.NextPageId);
        Assert.Equal(PageType.Empty, page.PageType);
    }

    [Fact]
    public void SetOneRow()
    {
        TableSerializer ser = new TableSerializer();
        var page = new Page(ser, 1);

        // Add row
        page.SetRow(TableIndexEnum.BTREE_KEY, 0, GetTableRow());
        Assert.Equal(2, page.TableCount);
        Assert.Single(page.GetTable(TableIndexEnum.BTREE_KEY));
    }

    [Fact]
    public void InsertOneRow()
    {
        TableSerializer ser = new TableSerializer();
        var page = new Page(ser, 1);

        // Insert row
        page.InsertRow(TableIndexEnum.BTREE_KEY, 0, GetTableRow());
        Assert.Equal(2, page.TableCount);
        Assert.Single(page.GetTable(TableIndexEnum.BTREE_KEY));
    }

    [Fact]
    public void InsertRowOutOfRange()
    {
        TableSerializer ser = new TableSerializer();
        var page = new Page(ser, 1);

        // Insert row out of bounds either side
        Assert.Throws<ArgumentOutOfRangeException>(() => { page.InsertRow(TableIndexEnum.BTREE_KEY, -1, GetTableRow()); });
        Assert.Throws<ArgumentOutOfRangeException>(() => { page.InsertRow(TableIndexEnum.BTREE_KEY, 1, GetTableRow()); });
    }

    [Fact]
    public void SetRowOutOfRange()
    {
        TableSerializer ser = new TableSerializer();
        var page = new Page(ser, 1);

        // Insert row out of bounds either side
        Assert.Throws<ArgumentOutOfRangeException>(() => { page.SetRow(TableIndexEnum.BTREE_KEY, -1, GetTableRow()); });
        Assert.Throws<ArgumentOutOfRangeException>(() => { page.SetRow(TableIndexEnum.BTREE_KEY, 1, GetTableRow()); });
    }

    [Fact]
    public void DeleteRowOutOfRange()
    {
        TableSerializer ser = new TableSerializer();
        var page = new Page(ser, 1);

        // Insert row out of bounds either side
        Assert.Throws<ArgumentOutOfRangeException>(() => { page.DeleteRow(TableIndexEnum.BTREE_KEY, -1); });
        Assert.Throws<ArgumentOutOfRangeException>(() => { page.DeleteRow(TableIndexEnum.BTREE_KEY, 1); });
    }

    [Fact]
    public void InsertTableOutOfRange()
    {
        TableSerializer ser = new TableSerializer();
        var page = new Page(ser, 1);

        // Insert row out of bounds either side
        Assert.Throws<ArgumentOutOfRangeException>(() => { page.InsertTable(TableIndexEnum.BTREE_CHILD); });
    }

    [Fact]
    public void DeleteTableOutOfRange()
    {
        TableSerializer ser = new TableSerializer();
        var page = new Page(ser, 1);

        // Insert row out of bounds either side
        Assert.Throws<ArgumentOutOfRangeException>(() => { page.DeleteTable(TableIndexEnum.BTREE_CHILD); });
    }

    [Fact]
    public void InitialiseHeader()
    {
        TableSerializer ser = new TableSerializer();
        var page = new Page(ser, 1);    // implicitly calls InitialiseHeader
        Assert.Equal(1, page.TableCount);
    }

    [Fact]
    public void InsertAndDeleteTables()
    {
        TableSerializer ser = new TableSerializer();
        var page = new Page(ser, 1);
        Assert.Equal(1, page.TableCount);

        // Create key table
        page.InsertTable(TableIndexEnum.BTREE_KEY);
        Assert.Equal(2, page.TableCount);

        // Delete the key table
        page.DeleteTable(TableIndexEnum.BTREE_KEY);
        Assert.Equal(1, page.TableCount);
    }
}