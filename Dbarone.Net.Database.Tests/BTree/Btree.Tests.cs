using System.Collections.Generic;
using System.Linq;
using Dbarone.Net.Database;
using Xunit;

public class BTreeTests
{
    private Btree InitialiseBtree(int? order = null)
    {
        var ph = new PageHydrater();
        var ts = new TableSerializer();
        var bm = new MemoryBufferManager(ph, ts, 8192, TextEncoding.UTF8);
        var s = new TableSchema();
        s.AddAttribute("number", DocumentType.Integer, false, true);
        var bt = new Btree(bm, s, order);
        return bt;
    }

    [Theory]
    [InlineData([new int[] { 1 }])]
    [InlineData([new int[] { 1, 2 }])]
    [InlineData([new int[] { 2, 1 }])]
    public void Insert(int[] data)
    {
        var bt = InitialiseBtree();
        foreach (var item in data)
        {
            TableRow r = new TableRow();
            r["number"] = item;
            bt.Insert(r);
        }

        List<long> values = new List<long>();
        var traverseResult = bt.Traverse(values, (values, row) => { values.Add(row["number"].AsInteger); return values; });
        Assert.Equal(data.Count(), traverseResult.Count());
    }

    [Theory]
    [InlineData([new int[] { 1, 2, 3, 4 }, 5, 1])]
    [InlineData([new int[] { 1, 2, 3, 4, 5 }, 5, 1])]
    public void Insert_With_PageSplit(int[] data, int order, int expectedPageCount)
    {
        var bt = InitialiseBtree(order);
        foreach (var item in data)
        {
            TableRow r = new TableRow();
            r["number"] = item;
            bt.Insert(r);
        }
        var totalPages = bt.BufferManager.Count;
        Assert.Equal(expectedPageCount, totalPages);
    }
}