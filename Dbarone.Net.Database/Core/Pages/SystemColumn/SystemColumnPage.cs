namespace Dbarone.Net.Database;
using Dbarone.Net.Assertions;
using Dbarone.Net.Proxy;

/// <summary>
/// System page storing data about columns.
/// </summary>
public class SystemColumnPage : Page
{
    public override Type PageDataType { get { return typeof(SystemColumnPageData); } }
    public override Type PageHeaderType { get { return typeof(PageHeader); } }
    public override IPageHeader Headers() { return (IPageHeader)this._headers; }
    public override IEnumerable<SystemColumnPageData> Data() { return (this._data.Select(d => (SystemColumnPageData)d)); }

    public SystemColumnPage(int pageId) : base(pageId, null, PageType.SystemColumn)
    {
        Assert.Equals(this._headers.PageType, PageType.SystemColumn);
    }

    public SystemColumnPage() { }
    
    public override void CreateHeaderProxy()
    {
        // Decorate the header with IsDirtyInterceptor
        // This interceptor will set the IsDirty flag whenever any header property changes.
        var generator = new ProxyGenerator<IPageHeader>();
        generator.Interceptor = Page.IsDirtyInterceptor;
        this._headers = generator.Decorate((IPageHeader)this._headers!);
    }
}