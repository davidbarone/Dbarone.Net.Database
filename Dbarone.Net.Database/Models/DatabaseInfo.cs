using Dbarone.Net.Document;

namespace Dbarone.Net.Database;

/// <summary>
/// Public information about a database.
/// </summary>
public class DatabaseInfo
{
    public string Magic { get; set; } = default!;
    public Byte Version { get; set; }
    public DateTime CreationTime { get; set; }
    public int PageCount { get; set; }
    public int NextObjectId { get; set; }
    public int FirstTablesPageId { get; set; }
    public int LastTablesPageId { get; set; }
    public TextEncoding TextEncoding { get; set; }
}