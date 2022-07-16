namespace Dbarone.Net.Database;

/// <summary>
/// System page storing data about columns.
/// </summary>
public class SystemColumnPage : Page
{
    protected override Type PageDataType { get { return typeof(SystemColumnPageData); } }
    protected override Type PageHeaderType { get { return typeof(PageHeader); } }
    public override PageHeader Headers() { return (PageHeader)this._headers; }
    public override IEnumerable<SystemColumnPageData> Data() { return (IEnumerable<SystemColumnPageData>)this._data; }

    public SystemColumnPage(int pageId, PageBuffer buffer) : base(pageId, buffer) { }
}