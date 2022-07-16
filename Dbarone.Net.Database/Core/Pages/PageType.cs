/// <summary>
/// The page type.
/// </summary>
public enum PageType : int
{
    /// <summary>
    /// A page type that stores raw leaf level data.
    /// </summary>
    Data = 1,
    /// <summary>
    /// A page type that stores index information.
    /// </summary>
    Index = 2,

    /// <summary>
    /// A page type that stores the database metadata.
    /// </summary>
    Boot = 3,

    /// <summary>
    /// A page type that stores table metadata.
    /// </summary>
    SystemTable = 4,

    /// <summary>
    /// A page type that stores table metadata.
    /// </summary>
    SystemColumn = 5,
}