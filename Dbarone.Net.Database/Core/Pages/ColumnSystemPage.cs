namespace Dbarone.Net.Database;

/// <summary>
/// System page storing data about columns.
/// </summary>
public class ColumnSystemPage : Page
{
    public ColumnSystemPage(int pageId, PageBuffer buffer) : base(pageId, buffer) { }
    public IEnumerable<ColumnInfo> Columns { get; set; } = default!;
}