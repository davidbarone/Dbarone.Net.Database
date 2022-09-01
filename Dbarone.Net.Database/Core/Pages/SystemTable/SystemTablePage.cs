namespace Dbarone.Net.Database;
using Dbarone.Net.Assertions;
using Dbarone.Net.Proxy;

/// <summary>
/// System page storing data about tables.
/// </summary>
public class SystemTablePage : Page
{
    protected override Type PageDataType { get { return typeof(SystemTablePageData); } }
    protected override Type PageHeaderType { get { return typeof(PageHeader); } }
    public override IPageHeader Headers() { return (IPageHeader)this._headers; }
    public override IEnumerable<SystemTablePageData> Data() { return (this._data.Select(d => (SystemTablePageData)d)); }
    public SystemTablePage(uint pageId, PageBuffer buffer) : base(pageId, buffer, PageType.SystemTable)
    {
        Assert.Equals(this._headers.PageType, PageType.SystemTable);
    }

    public override void CreateHeaderProxy()
    {
        // Decorate the header with IsDirtyInterceptor
        // This interceptor will set the IsDirty flag whenever any header property changes.
        var generator = new ProxyGenerator<IPageHeader>();
        generator.Interceptor = Page.IsDirtyInterceptor;
        this._headers = generator.Decorate((IPageHeader)this._headers!);
    }
}