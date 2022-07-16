namespace Dbarone.Net.Database;
using Dbarone.Net.Assertions;

/// <summary>
/// First page in any database. Stores database-wide parameters. Data rows are pointers to database objects.
/// </summary>
public class BootPage : Page<BootPageHeader, BootPageData>
{
    public BootPage (int pageId, PageBuffer buffer) : base(pageId, buffer) {
        Assert.Equals(this.Headers.PageType, PageType.Boot);
    }
}