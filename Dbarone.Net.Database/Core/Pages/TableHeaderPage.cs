namespace Dbarone.Net.Database;

/// <summary>
/// Stores table metadata. One page (first page) per table. Data rows store information about table columns.
/// </summary>
public class TableHeaderPage : Page
{
    public TableHeaderPage(int pageId, PageBuffer buffer):base(pageId, buffer){}
}