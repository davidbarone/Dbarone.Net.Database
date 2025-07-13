using System.Collections.Generic;
using Xunit;
using System;
using Dbarone.Net.Database;
using System.Linq;

public class TableMapperTests
{
    public class TableRowPoco
    {
        public int integer { get; set; }
        public string text { get; set; } = default!;
        public DateTime datetime { get; set; }
    }

    [Fact]
    public void TestMapIEnumerableDictionaryToTable()
    {
        List<Dictionary<string, object>> dictList = new List<Dictionary<string, object>>()
        {
            new Dictionary<string, object>()
            {
                {"integer", (long)1},
                {"text", "foo"},
                {"datetime", new DateTime(2000,1,1)}
            },
            new Dictionary<string, object>()
            {
                {"integer", (long)2},
                {"text", "bar"},
                {"datetime", new DateTime(2000,1,2)}
            }
        };

        TableMapper mapper = new TableMapper();
        var table = mapper.MapIEnumerableDictionaryToTable(dictList);

        Assert.Equal(2, table.Count);   // should have 2 rows.
        Assert.Equal("bar", table[0]["text"]);
    }

    [Fact]
    public void TestMapTableToIEnumerableDictionary()
    {
        Table t = new Table();
        TableRow r1 = new TableRow();
        r1["integer"] = (long)1;
        r1["text"] = "foo";
        r1["datetime"] = new DateTime(2000, 1, 1);

        TableRow r2 = new TableRow();
        r2["integer"] = (long)2;
        r2["text"] = "bar";
        r2["datetime"] = new DateTime(2000, 1, 2);

        t.Add(r1);
        t.Add(r2);

        TableMapper mapper = new TableMapper();
        var iEnumerableDict = mapper.MapTableToIEnumerableDictionary(t);

        Assert.Equal(2, iEnumerableDict.Count());   // should have 2 rows.
        Assert.Equal("bar", (string)iEnumerableDict.ToList()[1]["text"]);
    }
}