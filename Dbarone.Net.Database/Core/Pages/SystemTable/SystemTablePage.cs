namespace Dbarone.Net.Database;
using Dbarone.Net.Assertions;
using Dbarone.Net.Proxy;

/// <summary>
/// System page storing data about tables.
/// </summary>
public class SystemTablePage : Page
{
    public override Type PageDataType { get { return typeof(SystemTablePageData); } }
    public override Type PageHeaderType { get { return typeof(PageHeader); } }
    public override IPageHeader Headers() { return (IPageHeader)this._headers; }
    public SystemTablePage(int pageId) : base(pageId, null, PageType.SystemTable)
    {
        Assert.Equals(this.PageType, PageType.SystemTable);
    }

    public SystemTablePage() { }
    
    public override void CreateHeaderProxy()
    {
        // Decorate the header with IsDirtyInterceptor
        // This interceptor will set the IsDirty flag whenever any header property changes.
        var generator = new ProxyGenerator<IPageHeader>();
        generator.Interceptor = IsDirtyInterceptor;
        this._headers = generator.Decorate((IPageHeader)this._headers!);
    }
}