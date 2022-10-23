namespace Dbarone.Net.Database;
using Dbarone.Net.Assertions;
using Dbarone.Net.Proxy;

/// <summary>
/// Stores data that cannot fix in normal page (CLOB, BLOB)
/// </summary>
public class OverflowPage : Page
{
    public override Type PageDataType { get { return typeof(BufferPageData); } }
    public override Type PageHeaderType { get { return typeof(OverflowPageHeader); } }
    public override IOverflowPageHeader Headers() { return (IOverflowPageHeader)this._headers; }
    public override IEnumerable<BufferPageData> Data() { return (this._data.Select(d => (BufferPageData)d)); }

    public OverflowPage(int pageId) : base(pageId, null, PageType.Overflow)
    {
        Assert.Equals(this._headers.PageType, PageType.Overflow);
    }

    public OverflowPage() { }
    
    public override void CreateHeaderProxy()
    {
        // Decorate the header with IsDirtyInterceptor
        // This interceptor will set the IsDirty flag whenever any header property changes.
        var generator = new ProxyGenerator<IOverflowPageHeader>();
        generator.Interceptor = Page.IsDirtyInterceptor;
        this._headers = generator.Decorate((IOverflowPageHeader)this._headers!);
    }
}