using Dbarone.Net.Database;

public class PageHydrater : IPageHydrater
{
    public IBuffer Dehydrate(Page page, ITableSerializer serializer, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        // serialise data table
        var bytes = serializer.Serialize(page.Data, textEncoding);
        page.DataLength = bytes.Length;

        // serialise the header fields:
        GenericBuffer buffer = new GenericBuffer();
        buffer.Write((long)page.PageId);
        buffer.Write((long)page.PageType);
        buffer.Write(page.PrevPageId is null ? (long)-1 : (long)page.PrevPageId);
        buffer.Write(page.NextPageId is null ? (long)-1 : (long)page.NextPageId);
        buffer.Write(page.ParentPageId is null ? (long)-1 : (long)page.ParentPageId);
        buffer.Write(page.IsDirty);
        buffer.Write((long)page.DataLength);

        buffer.Position = 100;
        buffer.Write(bytes);
        return buffer;
    }

    public Page Hydrate(IBuffer buffer, ITableSerializer serializer, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        buffer.Position = 0;
        Page page = new Page();

        page.PageId = (int)buffer.ReadInt64();
        page.PageType = (PageType)buffer.ReadInt64();
        int next = (int)buffer.ReadInt64();
        page.PrevPageId = next == -1 ? null : next;
        next = (int)buffer.ReadInt64();
        page.NextPageId = next == -1 ? null : next;
        next = (int)buffer.ReadInt64();
        page.ParentPageId = next == -1 ? null : next;
        page.IsDirty = buffer.ReadBool();
        page.DataLength = (int)buffer.ReadInt64();

        // write page table
        buffer.Position = 100;
        var bytes = buffer.ReadBytes(page.DataLength);

        page.Data = serializer.Deserialize(bytes, textEncoding);
        return page;
    }
}