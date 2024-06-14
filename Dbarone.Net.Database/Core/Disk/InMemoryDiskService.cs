using Dbarone.Net.Database;

/// <summary>
/// Memory only <see cref="IDiskService"/> implementation.
/// </summary>
public class InMemoryDiskService : IDiskService
{
    public int PageCount => throw new NotImplementedException();

    public void ClearPage(int pageId)
    {
        throw new NotImplementedException();
    }

    public int CreatePage()
    {
        throw new NotImplementedException();
    }

    public PageBuffer ReadPage(int pageId)
    {
        throw new NotImplementedException();
    }

    public void WritePage(int pageId, PageBuffer page)
    {
        throw new NotImplementedException();
    }
}