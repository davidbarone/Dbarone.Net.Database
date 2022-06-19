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
            if (field.Size == -1)
            {
                field.Size = attr.MaxLength;
            }
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
        }
        else
        {
            property.Property.SetValue(this, value);
            if (!fieldName.Equals("IsDirty", StringComparison.OrdinalIgnoreCase))
            {
                WriteHeader("IsDirty", true);
            }

            // Write buffer
            if (property.Property.PropertyType == typeof(bool)) { _buffer.Write((bool)value, property.Start); }
            else if (property.Property.PropertyType == typeof(byte)) { _buffer.Write((byte)value, property.Start); }
            else if (property.Property.PropertyType == typeof(sbyte)) { _buffer.Write((sbyte)value, property.Start); }
            else if (property.Property.PropertyType == typeof(char)) { _buffer.Write((char)value, property.Start); }
            else if (property.Property.PropertyType == typeof(decimal)) { _buffer.Write((decimal)value, property.Start); }
            else if (property.Property.PropertyType == typeof(double)) { _buffer.Write((double)value, property.Start); }
            else if (property.Property.PropertyType == typeof(Single)) { _buffer.Write((Single)value, property.Start); }
            else if (property.Property.PropertyType == typeof(Int16)) { _buffer.Write((Int16)value, property.Start); }
            else if (property.Property.PropertyType == typeof(UInt16)) { _buffer.Write((UInt16)value, property.Start); }
            else if (property.Property.PropertyType == typeof(Int32)) { _buffer.Write((Int32)value, property.Start); }
            else if (property.Property.PropertyType == typeof(UInt32)) { _buffer.Write((UInt32)value, property.Start); }
            else if (property.Property.PropertyType == typeof(Int64)) { _buffer.Write((Int64)value, property.Start); }
            else if (property.Property.PropertyType == typeof(UInt64)) { _buffer.Write((UInt64)value, property.Start); }
            else if (property.Property.PropertyType == typeof(DateTime)) { _buffer.Write((DateTime)value, property.Start); }
            else if (property.Property.PropertyType == typeof(string)) { _buffer.Write((string)value, property.Start); }
            else if (property.Property.PropertyType == typeof(Guid)) { _buffer.Write((Guid)value, property.Start); }
            else if (property.Property.PropertyType == typeof(byte[])) { _buffer.Write((byte[])value, property.Start); }
            else { throw new Exception("Invalid property type."); }
        }
    }

    /// <summary>
    /// Reads header from buffer and updates a header property.
    /// </summary>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public void ReadHeader(string fieldName)
    {
        var property = Headers!.FirstOrDefault(f => f.Key.Equals(fieldName, StringComparison.OrdinalIgnoreCase)).Value;
        if (property == null)
        {
            throw new Exception($"Error reading header field: {fieldName}.");
        }
        else
        {
            // Write buffer
            if (property.Property.PropertyType == typeof(bool)) { property.Property.SetValue(this, _buffer.ReadBool(property.Start)); }
            else if (property.Property.PropertyType == typeof(byte)) { property.Property.SetValue(this, _buffer.ReadByte(property.Start)); }
            else if (property.Property.PropertyType == typeof(sbyte)) { property.Property.SetValue(this, _buffer.ReadSByte(property.Start)); }
            else if (property.Property.PropertyType == typeof(char)) { property.Property.SetValue(this, _buffer.ReadChar(property.Start)); }
            else if (property.Property.PropertyType == typeof(decimal)) { property.Property.SetValue(this, _buffer.ReadDecimal(property.Start)); }
            else if (property.Property.PropertyType == typeof(double)) { property.Property.SetValue(this, _buffer.ReadDouble(property.Start)); }
            else if (property.Property.PropertyType == typeof(Single)) { property.Property.SetValue(this, _buffer.ReadSingle(property.Start)); }
            else if (property.Property.PropertyType == typeof(Int16)) { property.Property.SetValue(this, _buffer.ReadInt16(property.Start)); }
            else if (property.Property.PropertyType == typeof(UInt16)) { property.Property.SetValue(this, _buffer.ReadUInt16(property.Start)); }
            else if (property.Property.PropertyType == typeof(Int32)) { property.Property.SetValue(this, _buffer.ReadInt32(property.Start)); }
            else if (property.Property.PropertyType == typeof(UInt32)) { property.Property.SetValue(this, _buffer.ReadUInt32(property.Start)); }
            else if (property.Property.PropertyType == typeof(Int64)) { property.Property.SetValue(this, _buffer.ReadInt64(property.Start)); }
            else if (property.Property.PropertyType == typeof(UInt64)) { property.Property.SetValue(this, _buffer.ReadUInt64(property.Start)); }
            else if (property.Property.PropertyType == typeof(DateTime)) { property.Property.SetValue(this, _buffer.ReadDateTime(property.Start)); }
            else if (property.Property.PropertyType == typeof(string)) { property.Property.SetValue(this, _buffer.ReadString(property.Start, property.Size)); }
            else if (property.Property.PropertyType == typeof(Guid)) { property.Property.SetValue(this, _buffer.ReadGuid(property.Start)); }
            else if (property.Property.PropertyType == typeof(byte[])) { property.Property.SetValue(this, _buffer.ReadBytes(property.Start, property.Size)); }
            else { throw new Exception("Invalid property type."); }
        }
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
    public static int PageSize = (int)Math.Pow(2, 13);   //8K (8192 bytes)
}