namespace Dbarone.Net.Database;

/// <summary>
/// Interface to provide page hydration and dehydration services.
/// </summary>
public interface IPageHydrater
{
    Page Hydrate(IBuffer buffer, ITableSerializer serializer, TextEncoding textEncoding = TextEncoding.UTF8);

    IBuffer Dehydrate(Page page, ITableSerializer serializer, TextEncoding textEncoding = TextEncoding.UTF8);
}