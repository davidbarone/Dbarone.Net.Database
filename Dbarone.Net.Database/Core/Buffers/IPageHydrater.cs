namespace Dbarone.Net.Database;

/// <summary>
/// Interface to provide page hydration and dehydration services.
/// </summary>
public interface IPageHydrater
{
    Page Hydrate(IBuffer buffer);

    IBuffer Dehydrate(Page page);
}