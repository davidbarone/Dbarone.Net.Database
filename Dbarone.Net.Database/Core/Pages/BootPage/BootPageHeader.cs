namespace Dbarone.Net.Database;

public interface IBootPageHeader : IPageHeader {
    string Magic { get; set; }
    byte Version { get; set; }
    uint PageCount { get; set; }
    DateTime CreationTime { get; set; }
}

/// <summary>
/// Headers for boot page type
/// </summary>
public class BootPageHeader : PageHeader , IBootPageHeader
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
    /// The number of pages in the database.
    /// </summary>
    public uint PageCount { get; set; }

    /// <summary>
    /// Database creation date/time
    /// </summary>
    public DateTime CreationTime { get; set; } = DateTime.Now;
}