namespace Dbarone.Net.Database;

/// <summary>
/// Headers for boot page type
/// </summary>
public class BootPageHeader : PageHeader
{
    /// <summary>
    /// Magic / file header.
    /// </summary>
    [PageHeader(1, 20)]
    public string Magic { get; set; } = "Dbarone.Net.Database";

    /// <summary>
    /// Database file format.
    /// </summary>
    [PageHeader(2)]
    public byte Version { get; set; } = 1;

    /// <summary>
    /// Last data page in database.
    /// </summary>
    [PageHeader(3)]
    public uint LastPageId { get; set; }

    /// <summary>
    /// Database creation date/time
    /// </summary>
    [PageHeader(4)]
    public DateTime CreationTime { get; set; }

    /// <summary>
    /// Link to tables metadata.
    /// </summary>
    public uint TablesPageId { get; set; }

    /// <summary>
    /// Link to columns metadata.
    /// </summary>
    public uint ColumnsPageId { get; set; }

}