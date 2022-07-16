namespace Dbarone.Net.Database;

/// <summary>
/// Headers for boot page type
/// </summary>
public class BootPageHeader : PageHeader
{
    /// <summary>
    /// Magic / file header.
    /// </summary>
    public string Magic { get; set; } = "Dbarone.Net.Database";

    /// <summary>
    /// Database file format.
    /// </summary>
    public byte Version { get; set; } = 1;

    /// <summary>
    /// Last data page in database.
    /// </summary>
    public uint LastPageId { get; set; }

    /// <summary>
    /// Database creation date/time
    /// </summary>
    public DateTime CreationTime { get; set; }
}