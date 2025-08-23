using System;
using Dbarone.Net.Database;
using Xunit;

public class PageHydraterTests
{
    private Page CreateTestPage(ITableSerializer tableSerializer)
    {
        var page = new Page(tableSerializer, 1);

        // header
        page.NextPageId = 2;
        page.PrevPageId = 3;
        page.ParentPageId = 4;
        page.IsDirty = true;
        var a = page.ParentPageId;

        // data
        Table t = new Table();
        TableRow row = new TableRow();
        row["integer"] = 123;
        row["text"] = "foobar";
        row["datetime"] = new DateTime(2000, 1, 1);
        t.Add(row);
        page.SetTable(TableIndexEnum.BTREE_KEY, t);
        page.TableCount = 2;
        return page;
    }

    [Fact]
    public void TestGetPageSize()
    {
        ITableSerializer ser = new TableSerializer();

        var page = CreateTestPage(ser);

        // dehydrate
        IPageHydrater hydrater = new PageHydrater();
        var result = hydrater.Dehydrate(page, ser, TextEncoding.UTF8);
        var page2 = hydrater.Hydrate(result.Buffer, ser, TextEncoding.UTF8);

        // Check that length of serialised string (expected length) equals GetPageSize (actual)
        Assert.Equal(page.TableCount, page2.TableCount);
        Assert.Equal(result.Length, page2.GetPageSize());
    }

    [Fact]
    public void TestDehydrateAndHydrate()
    {
        ITableSerializer ser = new TableSerializer();

        var page = CreateTestPage(ser);

        // dehydrate
        IPageHydrater hydrater = new PageHydrater();
        var buffer = hydrater.Dehydrate(page, ser, TextEncoding.UTF8).Buffer;

        // hydrate
        var page2 = hydrater.Hydrate(buffer, ser, TextEncoding.UTF8);

        Assert.Equal(page.PageId, page2.PageId);
        Assert.Equal(page.NextPageId, page2.NextPageId);
        Assert.Equal(page.PrevPageId, page2.PrevPageId);
        Assert.Equal(page.ParentPageId, page2.ParentPageId);
        Assert.Equal(page.IsDirty, page2.IsDirty);
        Assert.Equal(page.TableCount, page2.TableCount);
    }
}