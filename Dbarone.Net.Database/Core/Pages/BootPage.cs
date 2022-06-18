namespace Dbarone.Net.Database;
using Dbarone.Net.Assertions;

/// <summary>
/// First page in any database. Stores database-wide parameters. Data rows are pointers to database objects.
/// </summary>
public class BootPage : Page
{
    [PageHeaderField(1, 20)]
    public string Magic { get; set; } = "Dbarone.Net.Database";

    [PageHeaderField(2)]
    public byte Version { get; set; } = 1;

    [PageHeaderField(3)]
    public uint LastPageId { get; set; }

    [PageHeaderField(4)]
    public DateTime CreationTime { get; set; }

    public BootPage (int pageId, PageBuffer buffer) : base(pageId, buffer) {
        Assert.Equals(this.PageType, PageType.Boot);
    }
}