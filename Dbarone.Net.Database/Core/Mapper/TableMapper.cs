using Dbarone.Net.Assertions;
using System.Collections;
using Dbarone.Net.Extensions.Object;
using Dbarone.Net.Document;
using Dbarone.Net.Mapper;
using System.Data.SqlTypes;

namespace Dbarone.Net.Database;

/// <summary>
/// Base class for Serializer. A serializer perform serialize and deserialize functions to convert .NET objects to and from byte[] arrays.
/// </summary>
public class Serializer : ISerializer
{
    private int PageSize { get; set; }
    private TextEncoding TextEncoding { get; set; } = TextEncoding.UTF8;
    ObjectMapper Mapper { get; set; }

    public Serializer(int pageSize, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        this.PageSize = pageSize;
        this.TextEncoding = textEncoding;

        // Build mapper
        var conf = new MapperConfiguration()
                    .SetAutoRegisterTypes(true)
                    .RegisterResolvers<DocumentMemberResolver>()
                    .RegisterOperator<TableMapperOperator>()
                    .RegisterOperator<TableRowMapperOperator>();

        Mapper = new ObjectMapper(conf);
    }

    public Table Deserialize(byte[] buffer)
    {
        ITableSerializer ser = new TableSerializer();
        var table = ser.Deserialize(buffer, TextEncoding);
        return table;
    }

    public byte[] Serialize(Table table)
    {
        ITableSerializer ser = new TableSerializer();
        var bytes = ser.Serialize(table, TextEncoding);
        Assert.LessThanEquals(bytes.Length, PageSize);
        return bytes;
    }

    public object Deserialize(byte[] buffer, Type toType)
    {
        // Deserialise to TableCell
        var docValue = Deserialize(buffer);

        // Map to POCO
        try
        {
            var obj = Mapper.Map(toType, docValue);
            return obj;
        }
        catch (Exception ex)
        {
            var a = ex;
            return null;
        }
    }

    public byte[] Serialize(object obj)
    {
        // Map to TableCell
        var dict = (TableRow)Mapper.Map(typeof(TableRow), obj)!;

        // Deserialise to TableCell
        var bytes = Serialize(dict);
        Assert.LessThanEquals(bytes.Length, PageSize);
        return bytes;
    }

    public PageBuffer Serialize(Page page)
    {
        /*
        // Create DocumentArray for the cells
        var arr = page.CellBuffers.Select(cb => (DictionaryDocument)cb);
        DocumentArray da = new DocumentArray(arr);

        DictionaryDocument dict = new DictionaryDocument();
        dict["PageId"] = page.PageId;
        dict["PageType"] = (byte)page.PageType;
        dict["Header"] = (DictionaryDocument)Mapper.Map(typeof(DictionaryDocument), page.Header)!;
        dict["Cells"] = new DocumentArray(Mapper.Map<IEnumerable<object>, List<DictionaryDocument>>(page.Cells)!);

        // Deserialise to TableCell
        var bytes = Serialize(dict);
        Assert.LessThanEquals(bytes.Length, PageSize);
        return new PageBuffer(bytes);
        */
        return null;    // to do
    }

    public Page Deserialize(PageBuffer buffer)
    {
        /*
        var bytes = buffer.ToArray();
        var dict = Deserialize(bytes);

        Page page = new Page(dict["PageId"], (PageType)(int)dict["PageType"]);
        page.IsDirty = false;

        DictionaryDocument header = dict["Header"].AsDocument;
        DocumentArray arr = dict["Cells"].AsArray;

        if (page.PageType == PageType.Boot)
        {
            page.Header = Mapper.Map<DictionaryDocument, BootData>(dict["Header"].AsDocument)!;
        }

        page.HeaderBuffer = Serialize(header);
        page.CellBuffers = arr.Select(c => Serialize(c)).ToArray();

        return page;
        */
        return null;    // to do
    }

    public bool IsPageOverflow(Page page, Table? table = null, object? cell = null)
    {
        var remaining = PageSize;
        remaining -= 5;     // pageid is int and pagetype is byte
        if (table is not null)
        {
            // new data object to replace old one - calculate size and deduct from remaining
            remaining = remaining - Serialize(table).Length;
        }
        if (cell is not null)
        {
            // new cell
            remaining = remaining - Serialize(cell).Length;
        }
        if (remaining < 2)
        {
            // allow buffer of 2 for serialtype to expand.
            return false;
        }
        else
        {
            return true;
        }
    }
}
