namespace Dbarone.Net.Database;
using Dbarone.Net.Document;

/// <summary>
/// Headers for boot page type
/// </summary>
public class BootData
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
    /// Database creation date/time
    /// </summary>
    public DateTime CreationTime { get; set; } = DateTime.Now;

    /// <summary>
    /// Provides a mechanism to generate database-wide unique numbers.
    /// </summary>
    public int NextObjectId { get; set; }

    /// <summary>
    /// Root node for system tables btree.
    /// </summary>
    public int SystemTablesPageId { get; set; }

    /// <summary>
    /// Sets the text encoding for the database.
    /// </summary>
    public TextEncoding TextEncoding { get; set; }

    /// <summary>
    /// The page size for the database.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// The overflow threshold expressed as a percentage. The default is 25.
    /// </summary>
    public int OverflowThreshold { get; set; } = 25;

    /// <summary>
    /// Pointer to single-linked list of free pages.
    /// </summary>
    public int? FirstFreePageId { get; set; }
}