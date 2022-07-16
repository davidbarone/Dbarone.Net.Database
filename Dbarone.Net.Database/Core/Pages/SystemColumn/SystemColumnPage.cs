namespace Dbarone.Net.Database;

/// <summary>
/// System page storing data about columns.
/// </summary>
public class SystemColumnPage : Page<PageHeader, SystemColumnData>
{
    public SystemColumnPage(int pageId, PageBuffer buffer) : base(pageId, buffer) { }
}