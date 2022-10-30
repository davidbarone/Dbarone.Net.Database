namespace Dbarone.Net.Database;
using Dbarone.Net.Assertions;
using Dbarone.Net.Proxy;

/// <summary>
/// Stores leaf-level data for a table.
/// </summary>
public class DataPage : Page
{
    public override Type PageDataType { get { return typeof(DictionaryPageData); } }
    public override Type PageHeaderType { get { return typeof(DataPageHeader); } }
    public override IDataPageHeader Headers() { return (IDataPageHeader)this._headers; }

    public DataPage(int pageId, int? parentObjectId) : base(pageId, parentObjectId, PageType.Data)
    {
        Assert.NotNull(parentObjectId);
        Assert.Equals(this.PageType, PageType.Data);
    }

    public DataPage() { }

    public override void CreateHeaderProxy()
    {
        // Decorate the header with IsDirtyInterceptor
        // This interceptor will set the IsDirty flag whenever any header property changes.
        var generator = new ProxyGenerator<IDataPageHeader>();
        generator.Interceptor = Page.IsDirtyInterceptor;
        this._headers = generator.Decorate((IDataPageHeader)this._headers!);
    }
}