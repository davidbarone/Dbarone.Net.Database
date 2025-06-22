using System;
using Xunit;
using Dbarone.Net.Database;

namespace Dbarone.Net.Database.Tests;

public class TableSerializerTests
{
    [Fact]
    public void SerializeTableTest()
    {
        Table t = new Table();
        TableRow r = new TableRow();
        r["text"] = "foobar";
        r["integer"] = 123;
        r["datetime"] = new DateTime(2000, 01, 01);

        t.Add(r);

        ITableSerializer ser = new TableSerializer();

        // Serialize
        var bytes = ser.Serialize(t);

        // Deserialize
        var t2 = ser.Deserialize(bytes);

        Assert.NotNull(t2);
        Assert.Equal(t.Count, t2.Count);
        var r2 = t2[1];
        Assert.Equal("foobar", r2["text"].AsText);
        Assert.Equal(123, r2["integer"].AsInteger);
        Assert.Equal(new DateTime(2000, 01, 01), r2["datetime"].AsDateTime);
    }
}