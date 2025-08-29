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
    [InlineData([new int[] { 5, 4, 3, 2, 1 }, 4, 3, "(1|R) (K:1 C:2) 2|0 |2\r\n(0|L) (K:2 C:0) 1| 2|\r\n(2|L) (K:3 C:0) 3| 4| 5|"])]
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

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(10000)]
    public void RandomInsert(int numberOfInserts)
    {
        List<long> testData = new List<long>();
        for (int i = 0; i < numberOfInserts; i++)
        {
            testData.Add(new Random().Next());
        }

        var bt = InitialiseBtree(100);
        foreach (var item in testData)
        {
            TableRow r = new TableRow();
            r["number"] = item;
            bt.Insert(r);
        }

        List<long> values = new List<long>();
        var traverseResult = bt.Traverse(
            values,
            (values, page, i, nodeIndex) =>
            {
                if (page.GetHeader("LEAF").AsBoolean)
                {
                    values.Add(page.GetRow(TableIndexEnum.BTREE_KEY, i)["number"].AsInteger); return values;
                }
                return values;
            }
        );
        Assert.Equal(testData.Count(), traverseResult.Count());
        Assert.Equal(testData.Order().ToArray(), traverseResult.ToArray());
    }

    [Theory]
    [InlineData([new int[] { 5, 4, 3, 2, 1 }, 3, 3])]       // match
    [InlineData([new int[] { 5, 4, 3, 2, 1 }, 6, null])]    // no match
    public void Search(int[] data, int searchKey, int? expected)
    {
        var bt = InitialiseBtree(4);
        foreach (var item in data)
        {
            TableRow r = new TableRow();
            r["number"] = item;
            bt.Insert(r);
        }
        var location = bt.Search(searchKey);
        if (location is not null)
        {
            var row = bt.Get(location.Value);
            int? actual = row is not null ? (int)row["number"].AsInteger : null;
            Assert.Equal(expected, actual);
        }
        else
        {
            Assert.Null(expected);
        }
    }

    [Theory]
    [InlineData([new int[] { 1, 2, 3 }, 3, 2])]
    public void Delete(int[] keysToInsert, int keyToDelete, int expectedCount)
    {
        var bt = InitialiseBtree(4);
        foreach (var item in keysToInsert)
        {
            TableRow r = new TableRow();
            r["number"] = item;
            bt.Insert(r);
        }

        bt.Delete(new TableCell(keyToDelete));
        List<long> values = new List<long>();
        var traverseResult = bt.Traverse(values, (values, page, i, nodeIndex) => { values.Add(page.GetRow(TableIndexEnum.BTREE_KEY, i)["number"].AsInteger); return values; });
        Assert.Equal(expectedCount, traverseResult.Count());
    }

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(10000)]
    public void RandomInsertAndDelete(int numberOfInsertDeletes)
    {
        List<long> testData = new List<long>();
        for (int i = 0; i < numberOfInsertDeletes; i++)
        {
            testData.Add(new Random().Next());
        }

        var bt = InitialiseBtree(100);
        foreach (var item in testData)
        {
            TableRow r = new TableRow();
            r["number"] = item;
            bt.Insert(r);
        }

        foreach (var item in testData)
        {
            bt.Delete(new TableCell(item));
        }

        List<long> values = new List<long>();
        var traverseResult = bt.Traverse(
            values,
            (values, page, i, nodeIndex) =>
            {
                if (page.GetHeader("LEAF").AsBoolean)
                {
                    values.Add(page.GetRow(TableIndexEnum.BTREE_KEY, i)["number"].AsInteger); return values;
                }
                return values;
            }
        );
        Assert.Empty(traverseResult);
    }
}