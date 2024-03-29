namespace Dbarone.Net.Database;

public interface IBootPageHeader : IPageHeader {
    string Magic { get; set; }
    byte Version { get; set; }
    int PageCount { get; set; }
    DateTime CreationTime { get; set; }
    int NextObjectId { get; set; }
    int TablesPageId { get; set; }
    TextEncoding TextEncoding { get; set; }
    int? FirstFreePageId { get; set; }
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
    public int PageCount { get; set; }

    /// <summary>
    /// Database creation date/time
    /// </summary>
    public DateTime CreationTime { get; set; } = DateTime.Now;

    /// <summary>
    /// Provides a mechanism to generate database-wide unique numbers.
    /// </summary>
    public int NextObjectId { get; set; }

    /// <summary>
    /// First page id of system tables.
    /// </summary>
    public int TablesPageId { get; set; }

    /// <summary>
    /// Sets the text encoding for the database.
    /// </summary>
    public TextEncoding TextEncoding { get; set; }
    
    /// <summary>
    /// Pointer to single-linked list of free pages.
    /// </summary>
    public int? FirstFreePageId { get; set; }
}