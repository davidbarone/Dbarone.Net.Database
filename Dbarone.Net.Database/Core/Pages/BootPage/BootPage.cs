namespace Dbarone.Net.Database;
using Dbarone.Net.Assertions;
using Dbarone.Net.Proxy;

/// <summary>
/// First page in any database. Stores database-wide parameters. Data rows are pointers to database objects.
/// </summary>
public class BootPage : Page
{
    public override Type PageDataType { get { return typeof(BootPageData); } }
    public override Type PageHeaderType { get { return typeof(BootPageHeader); } }
    public override IBootPageHeader Headers() { return (IBootPageHeader)this._headers; }
    public override IEnumerable<BootPageData> Data() { return (this._data.Select(d => (BootPageData)d)); }

    public BootPage(int pageId) : base(pageId, null, PageType.Boot)
    {
        Assert.Equals(this._headers.PageType, PageType.Boot);
    }

    public BootPage() { }
    
    public override void CreateHeaderProxy()
    {
        // Decorate the header with IsDirtyInterceptor
        // This interceptor will set the IsDirty flag whenever any header property changes.
        var generator = new ProxyGenerator<IBootPageHeader>();
        generator.Interceptor = Page.IsDirtyInterceptor;
        this._headers = generator.Decorate((IBootPageHeader)this._headers!);
    }

    public int GetNextObjectId() {
        var id = this.Headers().NextObjectId;
        this.Headers().NextObjectId++;
        return id;
    }
}