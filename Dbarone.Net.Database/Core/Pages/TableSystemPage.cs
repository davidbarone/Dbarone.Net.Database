namespace Dbarone.Net.Database;

/// <summary>
/// System page storing data about tables.
/// </summary>
public class TableSystemPage : Page
{
    public TableSystemPage(int pageId, PageBuffer buffer):base(pageId, buffer){}
    
    public IEnumerable<TableInfo> Tables { get; set; }
}