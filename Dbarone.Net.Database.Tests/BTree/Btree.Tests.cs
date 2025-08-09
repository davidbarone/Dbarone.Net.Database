using System;
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
        var traverseResult = bt.Traverse(values, (values, page, i, nodeIndex) => { values.Add(page.GetRow(TableIndexEnum.BTREE_KEY, i)["number"].AsInteger); return values; });
        Assert.Equal(data.Count(), traverseResult.Count());
    }

    [Theory]
    [InlineData([new int[] { 1, 2, 3, 4 }, 4, 1, "(0|L) (K:4 C:0) 1| 2| 3| 4|"])]
    [InlineData([new int[] { 1, 2, 3, 4, 5 }, 4, 3, "(1|R) (K:1 C:2) 2|0 |2\r\n(0|L) (K:2 C:0) 1| 2|\r\n(2|L) (K:3 C:0) 3| 4| 5|"])]
    public void Insert_With_PageSplit(int[] data, int order, int expectedPageCount, string expectedTraverse)
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

        // Check rows are ordered. Use Traverse to get all rows in btree.
        List<string> results = new List<string>();
        bt.Traverse<List<string>>(results, (results, page, i, nodeIndex) =>
        {
            var pid = page.PageId;
            var rootId = bt.Root!.PageId;
            var type = page.GetHeader("LEAF").AsBoolean ? "L" : (page.PageId == rootId) ? "R" : "I";
            var keyCount = page.GetTable(TableIndexEnum.BTREE_KEY).Count();
            var childCount = (type != "L") ? (int)page.GetTable(TableIndexEnum.BTREE_CHILD).Count() : 0;
            var key = (i < keyCount) ? (int?)page.GetRow(TableIndexEnum.BTREE_KEY, i)["number"].AsInteger : null;
            var childId = (type != "L") ? (int?)page.GetRow(TableIndexEnum.BTREE_CHILD, i)["PID"].AsInteger : null;

            if (results.Count() <= nodeIndex)
            {
                results.Insert(nodeIndex, $"({pid}|{type}) (K:{keyCount} C:{childCount})");
            }

            // Add key / child info
            results[nodeIndex] += $" {key}|{childId}";
            return results;
        });
        var actual = string.Join(Environment.NewLine, results.ToArray());
        Assert.Equal(expectedTraverse, actual);
    }
}