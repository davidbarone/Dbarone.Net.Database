namespace Dbarone.Net.Database;
using Dbarone.Net.Extensions.Reflection;
using System.Reflection;

/// <summary>
/// Base class for all pages.
/// </summary>
public abstract class Page
{
    /// <summary>
    /// Information about the fields.
    /// </summary>
    public Dictionary<string, PageHeaderFieldInfo> Fields { get; set; }

    /// <summary>
    /// Constructor.
    /// </summary> 
    public Page()
    {
        // Get header fields
        var headerProps = this.GetType().GetPropertiesDecoratedBy<PageHeaderFieldAttribute>();
        List<PageHeaderFieldInfo> fields = new List<PageHeaderFieldInfo>();
        foreach(var item in headerProps) {
            var attr = (PageHeaderFieldAttribute)item.GetCustomAttributes(typeof(PageHeaderFieldAttribute), false).First();
            var field = new PageHeaderFieldInfo();
            field.Ordinal = attr.Ordinal;
            field.Property = item;
            field.Size = Types.Get()[item.PropertyType].Size;
        }
        int start = 0;
        foreach(var item in fields.OrderBy(f=>f.Ordinal)){
            item.Start = start;
            start = start + item.Size;
        }
        this.Fields = fields.ToDictionary(key => key.Property.Name, value => value);
    }

    /// <summary>
    /// Writes header to buffer.
    /// </summary>
    /// <param name="fieldName"></param>
    /// <exception cref="Exception"></exception>
    public void WriteHeader(string fieldName) {
        var property = Fields!.FirstOrDefault(f => f.Key.Equals(fieldName, StringComparison.OrdinalIgnoreCase)).Value;
        if (property==null){
            throw new Exception($"Error writing header field: {fieldName}.");
        }
    }

    /// <summary>
    /// Reads header from buffer.
    /// </summary>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public object ReadHeader(string fieldName){

    }
}