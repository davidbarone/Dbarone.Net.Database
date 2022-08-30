namespace Dbarone.Net.Database;
using Dbarone.Net.Extensions.Reflection;
using Dbarone.Net.Assertions;
using Dbarone.Net.Proxy;

/// <summary>
/// Base class for all pages.
/// </summary>
public class Page
{
    private PageBuffer _buffer;
    protected IPageHeader _headers;
    protected IList<PageData> _data;

    protected virtual Type PageHeaderType { get { throw new NotImplementedException("Not implemented."); } }

    protected virtual Type PageDataType { get { throw new NotImplementedException("Not implemented."); } }

    /// <summary>
    /// Header information.
    /// </summary>
    public virtual IPageHeader Headers() { throw new NotImplementedException("Not implemented."); }

    /// <summary>
    /// Data within the page. The data is indexed using the slots array
    /// </summary>
    public virtual IEnumerable<PageData> Data() { throw new NotImplementedException("Not implemented."); }

    /// <summary>
    /// The slot indexes.
    /// </summary>
    public IList<ushort> Slots { get; set; }

    /// <summary>
    /// Returns the structure of the data rows on this page type.
    /// </summary>
    //protected virtual IEnumerable<ColumnInfo>? DataRowStucture { get { return null; } }

    private void Hydrate(PageBuffer buffer)
    {
        // Hydrate Headers
        this._headers = (PageHeader)Serializer.Deserialize(this.PageHeaderType, buffer.ToArray());

        // Hydrate slots
        var slotIndex = Page.PageSize - 2;
        for (int slot = 0; slot < this._headers.SlotsUsed; slot++)
        {
            var dataIndex = buffer.ReadUInt16(slotIndex);
            this.Slots.Add(dataIndex);

            // add data
            // totalLength is the second UInt from start.
            var totalLength = buffer.ReadUInt16(dataIndex + Types.GetByDataType(DataType.UInt16).Size);
            var b = buffer.Slice(dataIndex, totalLength);
            var item = (PageData)Serializer.Deserialize(this.PageDataType, b);
            this._data.Add(item);

            slotIndex = slotIndex - 2;
        }
    }

    /// <summary>
    /// Instantiates a new Page object.
    /// </summary>
    /// <param name="pageId">The page id.</param>
    /// <param name="buffer"></param>
    public Page(int pageId, PageBuffer buffer, PageType pageType)
    {
        this._buffer = buffer;
        this._data = new List<PageData>();
        this.Slots = new List<ushort>();

        if (!buffer.IsEmpty())
        {
            Hydrate(buffer);
        } else {
            this._headers = (IPageHeader)Activator.CreateInstance(this.PageHeaderType)!;
        }


        // Decorate the header with IsDirtyInterceptor
        // This interceptor will set the IsDirty flag whenever any header property changes.
        CreateHeaderProxy();

        if (buffer.IsEmpty()){
            // If new page, set defaults. These will automatically set the IsDirty flag as interceptor created above.
            this._headers.PageId = pageId;
            this._headers.PageType = pageType;
            this._headers.SlotsUsed = 0;
        }

        Assert.Equals(pageId, this._headers.PageId);
    }

    /// <summary>
    /// Implement in a subclass to create header proxy.
    /// </summary>
    /// <param name="headers"></param>
    /// <returns></returns>
    public virtual void CreateHeaderProxy() { throw new NotImplementedException(); }

    /// <summary>
    /// Adds data row to the page.
    /// </summary>
    /// <param name="row"></param>
    public void AddDataRow(object row)
    {
        this.Headers().SlotsUsed++;

    }

    /// <summary>
    /// Sets the IsDirty to true if any setter is called.
    /// </summary>
    /// <param name="interceptorArgs"></param>
    public static void IsDirtyInterceptor<TPageHeaderInterface>(InterceptorArgs<TPageHeaderInterface> interceptorArgs) where TPageHeaderInterface : IPageHeader
    {
        // Change MakeSound behaviour on all animals
        if (interceptorArgs.BoundaryType == BoundaryType.After && interceptorArgs.TargetMethod.Name.Substring(0, 4) == "set_")
        {
            interceptorArgs.Target.IsDirty = true;
        }
    }

    /// <summary>
    /// Returns the current page as a PageBuffer. Used to write dirty pages back to disk
    /// </summary>
    /// <returns></returns>
    public PageBuffer ToPageBuffer()
    {
        byte[] b = new byte[8192];
        PageBuffer buffer = new PageBuffer(b, this._headers.PageId);

        // Headers
        var headerBytes = Serializer.Serialize(this.Headers());
        Assert.NotGreaterThan(96, headerBytes.Length);
        buffer.Write(headerBytes, 0);

        // Serialize slots
        var data = this.Data().ToList();
        var slotIndex = Page.PageSize - 2;
        for (int slot = 0; slot < this._headers.SlotsUsed; slot++)
        {
            var dataIndex = this.Slots[slot];
            buffer.Write(dataIndex, slotIndex);

            // Serialize data
            // totalLength is the second UInt from start.
            var dataBytes = Serializer.Serialize(data[slot]);
            buffer.Write(dataBytes, dataIndex);

            slotIndex = slotIndex - 2;
        }
        return buffer;
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