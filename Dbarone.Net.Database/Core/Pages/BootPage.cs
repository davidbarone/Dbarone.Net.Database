namespace Dbarone.Net.Database;

public class BootPage
{
    [PageHeaderField(1)]
    public string Header { get; set; } = "Dbarone.Net.Database";

    [PageHeaderField(2)]
    public byte Version { get; set; } = 1;

    [PageHeaderField(3)]
    public uint LastPageId { get; set; }

    [PageHeaderField(4)]
    public DateTime CreationTime { get; set; }

}