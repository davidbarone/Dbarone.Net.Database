/// <summary>
/// The page type.
/// </summary>
public enum PageType : Byte
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

    /// <summary>
    /// Stores large data which spills over 1 page in size.
    /// </summary>
    Overflow = 6,

    /// <summary>
    /// A page that no longer holds any data. An unused page. Pages may become unused if all data is deleted from the page.
    /// </summary>
    Free

}