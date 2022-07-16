namespace Dbarone.Net.Database;
using Dbarone.Net.Extensions.Reflection;
using Dbarone.Net.Assertions;

/// <summary>
/// Base class for all pages.
/// </summary>
public class Page
{
    private PageBuffer _buffer;
    protected PageHeader _headers;
    protected IList<PageData> _data;

    protected virtual Type PageHeaderType { get { throw new NotImplementedException("Not implemented."); } }

    protected virtual Type PageDataType { get { throw new NotImplementedException("Not implemented."); } }

    /// <summary>
    /// Header information.
    /// </summary>
    public virtual PageHeader Headers() { throw new NotImplementedException("Not implemented."); }

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
    public Page(int pageId, PageBuffer buffer)
    {
        this._buffer = buffer;
        this._data = new List<PageData>();
        this.Slots = new List<ushort>();
        Hydrate(buffer);
        Assert.Equals(pageId, this._headers.PageId);

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