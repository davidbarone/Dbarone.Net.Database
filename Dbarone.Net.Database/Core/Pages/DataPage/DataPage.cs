namespace Dbarone.Net.Database;
using Dbarone.Net.Assertions;
using Dbarone.Net.Proxy;

/// <summary>
/// Stores leaf-level data for a table.
/// </summary>
public class DataPage : Page
{
    protected override Type PageDataType { get { return typeof(DictionaryPageData); } }
    protected override Type PageHeaderType { get { return typeof(DataPageHeader); } }
    public override IDataPageHeader Headers() { return (IDataPageHeader)this._headers; }
    public override IEnumerable<DictionaryPageData> Data() { return (this._data.Select(d => (DictionaryPageData)d)); }

    public DataPage(int pageId, PageBuffer buffer, BufferManager bufferManager) : base(pageId, buffer, PageType.Data, bufferManager)
    {
        Assert.Equals(this._headers.PageType, PageType.Data);
    }

    public override void CreateHeaderProxy()
    {
        // Decorate the header with IsDirtyInterceptor
        // This interceptor will set the IsDirty flag whenever any header property changes.
        var generator = new ProxyGenerator<IDataPageHeader>();
        generator.Interceptor = Page.IsDirtyInterceptor;
        this._headers = generator.Decorate((IDataPageHeader)this._headers!);
    }

    protected override IEnumerable<ColumnInfo> GetDataColumns()
    {
        // Get the column structure from the parent -> columns
        var systemTablePage = this._bufferManager.GetPage<SystemTablePage>(1);
        var table = systemTablePage.Data().First(d => d.PageId == this.Headers().PageId);
        var systemColumnPage = this._bufferManager.GetPage<SystemColumnPage>(table.ColumnPageId);
        var mapper = Mapper.ObjectMapper<SystemColumnPageData, ColumnInfo>.Create();
        return mapper.MapMany(systemColumnPage.Data());
    }
}