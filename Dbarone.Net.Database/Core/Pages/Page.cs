namespace Dbarone.Net.Database;
using Dbarone.Net.Extensions.Reflection;

/// <summary>
/// Base class for all pages.
/// </summary>
public class Page
{
    private PageBuffer _buffer;
    
    [PageHeaderAttribute(1)]
    public PageType PageType { get; set; }

    [PageHeaderAttribute(2)]
    public int PageId { get; set; }

    [PageHeaderAttribute(3)]
    public int PrevPageId { get; set; }

    [PageHeaderAttribute(4)]
    public int NextPageId { get; set; }

    [PageHeaderAttribute(5)]
    public int SlotsUsed { get; set; }

    [PageHeaderAttribute(6)]
    public int TransactionId { get; set; }

    [PageHeaderAttribute(7)]
    public bool IsDirty { get; set; }


    /// <summary>
    /// Information about the fields.
    /// </summary>
    public Dictionary<string, PageHeaderInfo> Headers { get; set; }

    /// <summary>
    /// Create
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="pageId"></param>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static T Create<T>(int pageId, PageBuffer buffer) where T : Page
    {
        if (typeof(T) == typeof(BootPage)) return (T)(object)new BootPage(pageId, buffer);
        throw new Exception("Unable to create a new page.");
    }

    /// <summary>
    /// Instantiates a new Page object.
    /// </summary>
    /// <param name="pageId">The page id.</param>
    /// <param name="buffer"></param>
    public Page(int pageId, PageBuffer buffer)
    {
        this._buffer = buffer;

        // Get header fields
        var headerProps = this.GetType().GetPropertiesDecoratedBy<PageHeaderAttribute>();
        List<PageHeaderInfo> fields = new List<PageHeaderInfo>();
        foreach (var item in headerProps)
        {
            var attr = (PageHeaderAttribute)item.GetCustomAttributes(typeof(PageHeaderAttribute), false).First();
            var field = new PageHeaderInfo();
            field.Ordinal = attr.Ordinal;
            field.Property = item;
            field.Size = Types.GetByType(item.PropertyType).Size;
        }
        int start = 0;
        foreach (var item in fields.OrderBy(f => f.Ordinal))
        {
            item.Start = start;
            start = start + item.Size;
        }
        this.Headers = fields.ToDictionary(key => key.Property.Name, value => value);
    }

    /// <summary>
    /// Writes header to buffer.
    /// </summary>
    /// <param name="fieldName"></param>
    /// <exception cref="Exception"></exception>
    public void WriteHeader(string fieldName, object value)
    {
        var property = Headers!.FirstOrDefault(f => f.Key.Equals(fieldName, StringComparison.OrdinalIgnoreCase)).Value;
        if (property == null)
        {
            throw new Exception($"Error writing header field: {fieldName}.");
        } else {
            property.Property.SetValue(this, value);
            WriteHeader("IsDirty", true);
            // Write buffer
            //this._buffer.Write(value);
        }
    }

    /// <summary>
    /// Reads header from buffer.
    /// </summary>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public object ReadHeader(string fieldName)
    {
        return null;

    }

    /// <summary>
    /// Create a new page
    /// </summary>
    /// <param name="pageId"></param>
    /// <param name="buffer"></param>
    public static void Create(int pageId, PageBuffer buffer)
    {
        
    }

    /// <summary>
    /// The page size for all pages
    /// </summary>
    public static int PageSize = 2 ^ 13; //8K (8192 bytes)
}