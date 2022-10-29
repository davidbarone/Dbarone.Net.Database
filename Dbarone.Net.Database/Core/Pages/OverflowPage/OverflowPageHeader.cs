namespace Dbarone.Net.Database;

public interface IOverflowPageHeader : IPageHeader
{
}

/// <summary>
/// Headers for overflow page type
/// </summary>
public class OverflowPageHeader : PageHeader, IOverflowPageHeader
{
}