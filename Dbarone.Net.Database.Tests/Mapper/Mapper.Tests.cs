using System.Collections.Generic;
using Xunit;
using System;
using Dbarone.Net.Database;

public class MapperTests
{
    public class TableRowPoco
    {
        public int integer { get; set; }
        public string text { get; set; } = default!;
        public DateTime datetime { get; set; }
    }

    [Fact]
    public void TestGenericMapper()
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
        var table = mapper.MapFromDictionary(dictList);

        Assert.Equal(2, table.Count);   // should have 2 rows.
    }
}