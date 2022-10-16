namespace Dbarone.Net.Database;

/// <summary>
/// Interface for a buffer manager
/// </summary>
public interface IBufferManager
{
    /// <summary>
    /// Saves all dirty pages to disk.
    /// </summary>
    void SaveDirtyPages();

    /// <summary>
    /// Gets a page by id.
    /// </summary>
    /// <typeparam name="T">The type of page.</typeparam>
    /// <param name="pageId">The page id</param>
    /// <returns>Returns a page.</returns>
    T GetPage<T>(int pageId) where T : Page;

    /// <summary>
    /// Creates a new page.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T CreatePage<T>(int? parentObjectId = null, Page? linkedPage = null) where T : Page;

    byte[] SerialiseRow(object row, IEnumerable<ColumnInfo> columns);
}