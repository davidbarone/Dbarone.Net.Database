namespace Dbarone.Net.Database;

public interface IOverflowPageHeader : IPageHeader
{
    int BytesUsed { get; set; }
}

/// <summary>
/// Headers for overflow page type
/// </summary>
public class OverflowPageHeader : PageHeader, IOverflowPageHeader
{
    public int BytesUsed { get; set; }
}