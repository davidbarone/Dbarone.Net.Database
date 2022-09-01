/// <summary>
/// Public information about a database.
/// </summary>
public class DatabaseInfo {
    public string Magic { get; set; } = default!;
    public int Version { get; set; }
    public DateTime CreationTime { get; set; }
    public uint PageCount { get; set; }
}