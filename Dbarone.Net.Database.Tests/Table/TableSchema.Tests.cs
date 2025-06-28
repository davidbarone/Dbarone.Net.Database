using System;
using Xunit;

namespace Dbarone.Net.Database.Tests;

public class TableSchemaTests
{
    [Fact]
    public void TestSchemaIsValid()
    {
        Table t = new Table();
        t.Schema = new TableSchema();
        t.Schema.AddAttribute("integer", DocumentType.Integer, false);
        t.Schema.AddAttribute("text", DocumentType.Text, false);

        TableRow r = new TableRow();
        r["integer"] = 123;
        r["text"] = "foobar";

        t.Add(r);

        // manual validation
        Assert.True(t.IsValid);
    }

    [Fact]
    public void TestSchemaIsNotValidAttribute()
    {
        Table t = new Table();
        t.Schema = new TableSchema();
        t.Schema.AddAttribute("integer", DocumentType.Integer, false);
        t.Schema.AddAttribute("text", DocumentType.Text, false);

        TableRow r = new TableRow();
        r["integer"] = 123;
        r["datetime"] = DateTime.Now;   // "datetime" not in schema

        t.Add(r);

        // manual validation
        var exception = Assert.Throws<Exception>(() => t.IsValid);
        Assert.Equal("Attribute: text is not defined in the document.", exception.Message);
    }

    [Fact]
    public void TestSchemaIsNotValidAdditionalColumn()
    {
        Table t = new Table();
        t.Schema = new TableSchema();
        t.Schema.AddAttribute("integer", DocumentType.Integer, false);
        t.Schema.AddAttribute("text", DocumentType.Text, false);

        TableRow r = new TableRow();
        r["integer"] = 123;
        r["text"] = "foobar";
        r["datetime"] = DateTime.Now;   // "datetime" not in schema

        t.Add(r);

        // manual validation
        var exception = Assert.Throws<Exception>(() => t.IsValid);
        Assert.Equal("Attribute: datetime is not defined in the schema.", exception.Message);
    }

    [Fact]
    public void TestSchemaIsNotValidDataType()
    {
        Table t = new Table();
        t.Schema = new TableSchema();
        t.Schema.AddAttribute("integer", DocumentType.Integer, false);
        t.Schema.AddAttribute("text", DocumentType.Text, false);

        TableRow r = new TableRow();
        r["integer"] = 123;
        r["text"] = 123;   // invalid value - should be text

        t.Add(r);

        // manual validation
        var exception = Assert.Throws<Exception>(() => t.IsValid);
        Assert.Equal("Attribute: text contains value: 123 which is an invalid data type.", exception.Message);
    }

    [Fact]
    public void TestSchemaIsValidNull()
    {
        Table t = new Table();
        t.Schema = new TableSchema();
        t.Schema.AddAttribute("integer", DocumentType.Integer, false);
        t.Schema.AddAttribute("text", DocumentType.Text, true); // allow nulls

        TableRow r = new TableRow();
        r["integer"] = 123;
        r["text"] = null;   // is valid

        t.Add(r);

        // manual validation
        Assert.True(t.IsValid);
    }

    [Fact]
    public void TestSchemaIsValidMissing()
    {
        Table t = new Table();
        t.Schema = new TableSchema();
        t.Schema.AddAttribute("integer", DocumentType.Integer, false);
        t.Schema.AddAttribute("text", DocumentType.Text, true); // allow nulls

        TableRow r = new TableRow();
        r["integer"] = 123;

        // text column not set - however, column allows nulls, so is valid.

        t.Add(r);

        // manual validation
        Assert.True(t.IsValid);
    }

    [Fact]
    public void TestSchemaIsNotValidNull()
    {
        Table t = new Table();
        t.Schema = new TableSchema();
        t.Schema.AddAttribute("integer", DocumentType.Integer, false);
        t.Schema.AddAttribute("text", DocumentType.Text, false);

        TableRow r = new TableRow();
        r["integer"] = 123;
        r["text"] = null;   // invalid value - should be not null

        t.Add(r);

        // manual validation
        var exception = Assert.Throws<Exception>(() => t.IsValid);
        Assert.Equal("Attribute: text contains value: null which is an invalid data type.", exception.Message);
    }

    [Fact]
    public void TestSchemaIsNotValidMissing()
    {
        Table t = new Table();
        t.Schema = new TableSchema();
        t.Schema.AddAttribute("integer", DocumentType.Integer, false);
        t.Schema.AddAttribute("text", DocumentType.Text, false);

        TableRow r = new TableRow();
        r["integer"] = 123;

        // text column not set - not valid.

        t.Add(r);

        // manual validation
        var exception = Assert.Throws<Exception>(() => t.IsValid);
        Assert.Equal("Attribute: text is not defined in the document.", exception.Message);
    }

    [Fact]
    public void TestNoSchemaIsValid()
    {
        Table t = new Table();

        // no schema set - any data should be valid.

        TableRow r = new TableRow();
        r["integer"] = 123;
        r["text"] = null;
        r["datetime"] = DateTime.Now;

        t.Add(r);

        // manual validation
        Assert.True(t.IsValid);
    }
}