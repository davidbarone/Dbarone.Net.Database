using System;
using Dbarone.Net.Database;
using Xunit;

public class PageHydraterTests
{
    [Fact]
    public void TestDehydrateAndHydrate()
    {
        ITableSerializer ser = new TableSerializer();

        var page = new Page();

        // header
        page.PageId = 1;
        page.NextPageId = 2;
        page.PrevPageId = 3;
        page.ParentPageId = 4;
        page.IsDirty = true;

        // data
        Table t = new Table();
        TableRow row = new TableRow();
        row["integer"] = 123;
        row["text"] = "foobar";
        row["datetime"] = new DateTime(2000, 1, 1);
        t.Add(row);
        page.Data.Add(t);

        // dehydrate
        IPageHydrater hydrater = new PageHydrater();
        var buffer = hydrater.Dehydrate(page, ser, TextEncoding.UTF8);

        // hydrate
        var page2 = hydrater.Hydrate(buffer, ser, TextEncoding.UTF8);

        Assert.Equal(page.PageId, page2.PageId);
        Assert.Equal(page.NextPageId, page2.NextPageId);
        Assert.Equal(page.PrevPageId, page2.PrevPageId);
        Assert.Equal(page.ParentPageId, page2.ParentPageId);
        Assert.Equal(page.IsDirty, page2.IsDirty);
        Assert.Equal(page.Data.Count, page2.Data.Count);
    }
}