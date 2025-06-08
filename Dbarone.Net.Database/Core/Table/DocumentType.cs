namespace Dbarone.Net.Database;

/// <summary>
/// Represents data types allowed in Dbarone.Net.Database.
/// </summary>
public enum DocumentType : int
{
    /// <summary>
    /// Missing or unknown value.
    /// </summary>
    Null = 0,

    /// <summary>
    /// Whole numbers that can be positive or negative. Can vary from 1 to 8 bytes.
    /// </summary>
    Integer = 1,

    /// <summary>
    /// Floating point numbers, stored as 8-byte IEEE floating point number.
    /// </summary>
    Real = 2,

    /// <summary>
    /// A variable-length byte-array.
    /// </summary>
    Blob = 3,

    /// <summary>
    /// Text strings using a database encoding (UTF-8, UTF-16 etc)
    /// </summary>
    Text = 4,
}