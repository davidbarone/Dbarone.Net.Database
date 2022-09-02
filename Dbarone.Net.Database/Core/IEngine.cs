namespace Dbarone.Net.Database;

/// <summary>
/// Represents the internal functioning of the database. Not accessible to end-users.
/// </summary>
public interface IEngine : IDisposable
{
    /// <summary>
    /// Gets a page.
    /// </summary>
    /// <param name="pageId"></param>
    /// <returns></returns>
    T GetPage<T>(int pageId) where T : Page;
    void CheckPoint();
    TableInfo CreateTable<T>(string tableName);
}