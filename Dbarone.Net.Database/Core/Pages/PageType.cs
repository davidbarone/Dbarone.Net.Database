/// <summary>
/// The page type.
/// </summary>
public enum PageType : Byte
{
    /// <summary>
    /// Page type is empty page. These pages can be repurposed.
    /// </summary>
    Empty = 0,

    /// <summary>
    /// A page type that stores the database metadata.
    /// </summary>
    Boot = 1,

    /// <summary>
    /// A b-tree node page.
    /// </summary>
    Btree = 2,

    /// <summary>
    /// A page type that stores raw leaf level data.
    /// </summary>
    Data = 3,

    /// <summary>
    /// A page type that stores index information.
    /// </summary>
    Index = 4,

    /// <summary>
    /// A page type that stores table metadata.
    /// </summary>
    SystemTable = 5,

    /// <summary>
    /// A page type that stores table metadata.
    /// </summary>
    SystemColumn = 6,

    /// <summary>
    /// Stores large data which spills over 1 page in size.
    /// </summary>
    Overflow = 7,
}