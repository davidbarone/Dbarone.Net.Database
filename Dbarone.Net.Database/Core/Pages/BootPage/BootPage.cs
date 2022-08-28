namespace Dbarone.Net.Database;
using Dbarone.Net.Assertions;

/// <summary>
/// First page in any database. Stores database-wide parameters. Data rows are pointers to database objects.
/// </summary>
public class BootPage : Page
{
    protected override Type PageDataType { get { return typeof(BootPageData); } }
    protected override Type PageHeaderType { get { return typeof(BootPageHeader); } }
    public override BootPageHeader Headers() { return (BootPageHeader)this._headers; }
    public override IEnumerable<BootPageData> Data() { return (IEnumerable<BootPageData>)this._data; }

    public BootPage(int pageId, PageBuffer buffer) : base(pageId, buffer, PageType.Boot)
    {
        Assert.Equals(this._headers.PageType, PageType.Boot);
    }
}