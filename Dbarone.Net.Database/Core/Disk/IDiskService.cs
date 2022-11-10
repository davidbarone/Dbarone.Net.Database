namespace Dbarone.Net.Database;

/// <summary>
/// Services provided by the disk subsystem.
/// </summary>
public interface IDiskService {
    
    /// <summary>
    /// Gets the total page count of the database.
    /// </summary>
    int PageCount { get; }

    /// <summary>
    /// Create a page and return the new page id.
    /// </summary>
    /// <returns>The page id created.</returns>
    int CreatePage();

    /// <summary>
    /// Clears an existing page on disk.
    /// </summary>
    /// <param name="pageId">The page id to clear.</param>
    void ClearPage(int pageId);

    /// <summary>
    /// Reads a page from disk.
    /// </summary>
    /// <param name="pageId">The page id.</param>
    /// <returns>A PageBuffer object representing the page.</returns>
    PageBuffer ReadPage(int pageId);

    /// <summary>
    /// Writes / flushes a page back to disk.
    /// </summary>
    /// <param name="pageId">The page id.</param>
    /// <param name="page">The page data (as a PageBuffer).</param>
    void WritePage(int pageId, PageBuffer page);
}