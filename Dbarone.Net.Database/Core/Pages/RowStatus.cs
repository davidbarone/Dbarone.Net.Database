using Dbarone.Net.Database;

/// <summary>
/// Row status flags.
/// </summary>
[Flags]
public enum RowStatus : Byte {
    None = 0,
    Deleted = 1,
    Overflow = 2
}