using Dbarone.Net.Database;

/// <summary>
/// Row status flags.
/// </summary>
[Flags]
public enum RowStatus : Byte
{

    /// <summary>
    /// The row is normal / default status.
    /// </summary>
    Default = 0,

    /// <summary>
    /// The row has been deleted.
    /// </summary>
    Deleted = 1,

    /// <summary>
    /// The row is an overflow row.
    /// </summary>
    Overflow = 2,

    /// <summary>
    /// The row is a null value.
    /// </summary>
    //Null = 4
}