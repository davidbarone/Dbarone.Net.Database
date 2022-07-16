namespace Dbarone.Net.Database;
using Dbarone.Net.Extensions.Reflection;
using Dbarone.Net.Assertions;

/// <summary>
/// Base class for all pages.
/// </summary>
public class Page<THeader, TData> where THeader: PageHeader where TData: PageData
{
    private PageBuffer _buffer;

    /// <summary>
    /// Header information.
    /// </summary>
    public THeader Headers { get; set; } = default!;

    /// <summary>
    /// Data within the page. The data is indexed using the slots array
    /// </summary>
    public IList<TData> Data { get; set; }

    /// <summary>
    /// The slot indexes.
    /// </summary>
    public List<ushort> Slots { get; set; }

    /// <summary>
    /// Returns the structure of the data rows on this page type.
    /// </summary>
    //protected virtual IEnumerable<ColumnInfo>? DataRowStucture { get { return null; } }

    /// <summary>
    /// Create
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="pageId"></param>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static T Create<T>(int pageId, PageBuffer buffer) where T : class
    {
        if (typeof(T) == typeof(BootPage)) return (T)(object)new BootPage(pageId, buffer);
        if (typeof(T) == typeof(SystemTablePage)) return (T)(object)new SystemTablePage(pageId, buffer);
        if (typeof(T) == typeof(SystemColumnPage)) return (T)(object)new SystemColumnPage(pageId, buffer);
        throw new Exception("Unable to create a new page.");
    }

    private void Hydrate(PageBuffer buffer){
        // Hydrate Headers
        this.Headers = new EntitySerializer().Deserialize<THeader>(buffer.ToArray());

        // Hydrate slots
        var slotIndex = this.PageSize - 2;
        for (int slot = 0; slot < this.Headers.SlotsUsed; slot++) {
            var dataIndex = buffer.ReadUInt16(slotIndex);
            this.Slots.Add(dataIndex);

            // add data
            // totalLength is the second UInt from start.
            var totalLength = buffer.ReadUInt16(dataIndex + Types.GetByDataType(DataType.UInt16).Size);
            var b = buffer.Slice(dataIndex, totalLength);
            var item = new EntitySerializer().Deserialize<TData>(b);
            this.Data.Add(item);

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
        this.Data = new List<TData>();
        this.Slots = new List<ushort>();
        Hydrate(buffer);
        Assert.Equals(pageId, this.Headers.PageId);

    }

    /// <summary>
    /// Returns the current page as a PageBuffer. Used to write dirty pages back to disk
    /// </summary>
    /// <returns></returns>
    public PageBuffer ToPageBuffer() {
        byte[] b = new byte[8192];
        PageBuffer buffer = new PageBuffer(b, this.Headers.PageId);
        
        // Headers
        var headerBytes = new EntitySerializer().Serialize(this.Headers);
        Assert.NotGreaterThan(96, headerBytes.Length);
        buffer.Write(headerBytes, 0);

        // Serialize slots
        var slotIndex = this.PageSize - 2;
        for (int slot = 0; slot < this.Headers.SlotsUsed; slot++) {
            var dataIndex = this.Slots[slot];
            buffer.Write(dataIndex, slotIndex);

            // Serialize data
            // totalLength is the second UInt from start.
            var dataBytes = new EntitySerializer().Serialize(this.Data[slot]);
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
    public int PageSize = (int)Math.Pow(2, 13);   //8K (8192 bytes)
}