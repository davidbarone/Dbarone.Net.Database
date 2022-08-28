namespace Dbarone.Net.Database;

/// <summary>
/// System page storing data about tables.
/// </summary>
public class SystemTablePage : Page
{
    protected override Type PageDataType { get { return typeof(SystemTablePageData); } }
    protected override Type PageHeaderType { get { return typeof(PageHeader); } }
    public override PageHeader Headers() { return (PageHeader)this._headers; }
    public override IEnumerable<SystemTablePageData> Data() { return (IEnumerable<SystemTablePageData>)this._data; }    
    public SystemTablePage(int pageId, PageBuffer buffer):base(pageId, buffer, PageType.SystemTable){}

}