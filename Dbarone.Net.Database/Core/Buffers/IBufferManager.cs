namespace Dbarone.Net.Database;

/// <summary>
/// Interface for a buffer manager. A buffer manager caches pages read from disk, and offers a method to save dirty pages back to disk.
/// </summary>
public interface IBufferManager : IStorage
{
    public IPageHydrater PageHydrater { get; set; }
    public ITableSerializer TableSerializer { get; set; }


    /// <summary>
    /// Serializes pages to/from byte streams and provides other serialization services
    /// </summary>
    //public ISerializer Serializer { get; set; }

    public int PageSize { get; }

    /// <summary>
    /// Gets the maximum page id either in cache or on disk.
    /// </summary>
    public int MaxPageId { get; }

    /// <summary>
    /// Get the number of pages in the cache.
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// Create a new page in the buffers.
    /// </summary>
    /// <param name="pageType">The page type to create.</param>
    /// <returns>The page id created.</returns>
    Page Create();

    /// <summary>
    /// Gets a page.
    /// </summary>
    /// <param name="pageId">The page id.</param>
    /// <returns>Returns a page object from the cache or disk.</returns>
    Page Get(int pageId);

    /// <summary>
    /// Clears an existing page in the buffers. If the page is not already in the buffers, it will be read from storage.
    /// </summary>
    /// <param name="pageId">The page id to clear.</param>
    void Clear(int pageId);

    /// <summary>
    /// Saves all dirty pages to disk.
    /// </summary>
    void Save();

    /// <summary>
    /// Drops all buffers that are not dirty.
    /// </summary>
    void DropCleanBuffers();

}