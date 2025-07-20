namespace Dbarone.Net.Database;

/// <summary>
/// Defines storage operations.
/// </summary>
/// <remarks>
/// Storages operations are always per-page operations. All operations on this interface must also be single-threaded.
/// </remarks>
public interface IStorage
{
    /// <summary>
    /// Gets the number of pages currently in the storage.
    /// </summary>
    /// <returns></returns>
    long StoragePageCount();

    /// <summary>
    /// Reads a page from disk.
    /// </summary>
    /// <param name="pageId">The page id.</param>
    /// <returns>A GenericBuffer object representing the page.</returns>
    IBuffer StorageRead(int pageId);

    /// <summary>
    /// Writes / flushes a page back to disk.
    /// </summary>
    /// <param name="page">The page data (as a GenericBuffer).</param>
    void StorageWrite(IBuffer page);
}