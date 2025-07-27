using System.Collections.Generic;
using System.Linq;
using Dbarone.Net.Database;
using Xunit;

public class BTreeTests
{
    private Btree InitialiseBtree()
    {
        var ph = new PageHydrater();
        var ts = new TableSerializer();
        var bm = new MemoryBufferManager(8192, ph, ts, TextEncoding.UTF8);
        var s = new TableSchema();
        s.AddAttribute("number", DocumentType.Integer, false, true);
        var bt = new Btree(bm, s);
        return bt;
    }

    [InlineData([new int[] { 1 }])]
    [InlineData([new int[] { 1, 2 }])]
    [InlineData([new int[] { 2, 1 }])]
    [Theory]
    public void TestInsert(int[] data)
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
}