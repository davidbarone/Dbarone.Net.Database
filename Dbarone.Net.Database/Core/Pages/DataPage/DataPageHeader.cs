namespace Dbarone.Net.Database;

public interface IDataPageHeader : IPageHeader {
   
}

/// <summary>
/// Headers for data page type.
/// </summary>
public class DataPageHeader : PageHeader , IDataPageHeader
{

}