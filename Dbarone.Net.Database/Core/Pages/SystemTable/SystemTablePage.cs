namespace Dbarone.Net.Database;

/// <summary>
/// System page storing data about tables.
/// </summary>
public class SystemTablePage : Page<PageHeader, SystemTableData>
{
    public SystemTablePage(int pageId, PageBuffer buffer):base(pageId, buffer){}

}