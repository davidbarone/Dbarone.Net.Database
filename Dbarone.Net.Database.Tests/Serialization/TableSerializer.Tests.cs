using System;
using Xunit;
using Dbarone.Net.Database;

namespace Dbarone.Net.Database.Tests;

public class TableSerializerTests
{
    [Fact]
    public void SerializeTableNoSchemaTest()
    {
        Table t = new Table();

        TableRow r1 = new TableRow();
        r1["text"] = "foo";
        r1["integer"] = 123;
        r1["datetime"] = new DateTime(2000, 01, 01);

        t.Add(r1);

        TableRow r2 = new TableRow();
        r2["text"] = "bar";
        r2["integer"] = 456;
        r2["datetime"] = new DateTime(2001, 01, 01);

        t.Add(r2);

        ITableSerializer ser = new TableSerializer();

        // Serialize
        var result = ser.Serialize(t);

        // Deserialize
        var t2 = ser.Deserialize(result.Buffer).Table;

        Assert.NotNull(t2);
        Assert.Equal(t.Count, t2.Count);

        r1["text"] = t2[1]["text"];
        r1["integer"] = t2[1]["integer"];
        r1["datetime"] = t2[1]["datetime"];
    }

    [Fact]
    public void SerializeTableSchemaTest()
    {
        Table t = new Table();
        t.Schema = new TableSchema();
        t.Schema.AddAttribute("integer", DocumentType.Integer, false, false);
        t.Schema.AddAttribute("text", DocumentType.Text, false, false);
        t.Schema.AddAttribute("datetime", DocumentType.DateTime, false, false);

        TableRow r1 = new TableRow();
        r1["text"] = "foo";
        r1["integer"] = 123;
        r1["datetime"] = new DateTime(2000, 01, 01);

        t.Add(r1);

        TableRow r2 = new TableRow();
        r2["text"] = "bar";
        r2["integer"] = 456;
        r2["datetime"] = new DateTime(2001, 01, 01);

        t.Add(r2);

        ITableSerializer ser = new TableSerializer();

        // Serialize
        var result = ser.Serialize(t);

        // Deserialize
        var t2 = ser.Deserialize(result.Buffer).Table;

        Assert.NotNull(t2);
        Assert.Equal(t.Count, t2.Count);

        r1["text"] = t2[1]["text"];
        r1["integer"] = t2[1]["integer"];
        r1["datetime"] = t2[1]["datetime"];
    }
}