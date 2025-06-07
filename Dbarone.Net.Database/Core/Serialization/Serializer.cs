namespace Dbarone.Net.Database;

using Dbarone.Net.Assertions;
using System.Collections;
using Dbarone.Net.Extensions.Object;
using Dbarone.Net.Document;
using Dbarone.Net.Mapper;
using System.Data.SqlTypes;

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
                    .RegisterOperator<EnumerableDocumentValueMapperOperator>()
                    .RegisterOperator<MemberwiseDocumentValueMapperOperator>();

        Mapper = new ObjectMapper(conf);
    }

    public DocumentValue Deserialize(byte[] buffer)
    {
        IDocumentSerializer ser = new DocumentSerializer();
        var doc = ser.Deserialize(buffer, TextEncoding);
        return doc;
    }

    public byte[] Serialize(DocumentValue document)
    {
        IDocumentSerializer ser = new DocumentSerializer();
        var bytes = ser.Serialize(document, null, TextEncoding);
        Assert.LessThanEquals(bytes.Length, PageSize);
        return bytes;
    }

    public object Deserialize(byte[] buffer, Type toType)
    {
        // Deserialise to DocumentValue
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
        // Map to DocumentValue
        var dict = (DictionaryDocument)Mapper.Map(typeof(DictionaryDocument), obj)!;

        // Deserialise to DocumentValue
        var bytes = Serialize(dict);
        Assert.LessThanEquals(bytes.Length, PageSize);
        return bytes;
    }

    public PageBuffer Serialize(Page page)
    {
        // Create DocumentArray for the cells
        var arr = page.CellBuffers.Select(cb => (DictionaryDocument)cb);
        DocumentArray da = new DocumentArray(arr);

        DictionaryDocument dict = new DictionaryDocument();
        dict["PageId"] = page.PageId;
        dict["PageType"] = (byte)page.PageType;
        dict["Header"] = (DictionaryDocument)Mapper.Map(typeof(DictionaryDocument), page.Header)!;
        dict["Cells"] = new DocumentArray(Mapper.Map<IEnumerable<object>, List<DictionaryDocument>>(page.Cells)!);

        // Deserialise to DocumentValue
        var bytes = Serialize(dict);
        Assert.LessThanEquals(bytes.Length, PageSize);
        return new PageBuffer(bytes);
    }

    public Page Deserialize(PageBuffer buffer)
    {
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
    }

    public bool IsPageOverflow(Page page, DictionaryDocument? data = null, object? cell = null)
    {
        var remaining = PageSize;
        remaining -= 5;     // pageid is int and pagetype is byte
        if (data is not null)
        {
            // new data object to replace old one - calculate size and deduct from remaining
            remaining = remaining - Serialize(data).Length;
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
