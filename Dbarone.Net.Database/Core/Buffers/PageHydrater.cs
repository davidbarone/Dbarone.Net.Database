using Dbarone.Net.Database;

public class PageHydrater : IPageHydrater
{
    public (IBuffer Buffer, long Length) Dehydrate(Page page, ITableSerializer serializer, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        GenericBuffer buffer = new GenericBuffer();

        // serialise each of the tables (include data[0] which is header table)
        for (int i = 0; i < page.TableCount; i++)
        {
            var result = serializer.Serialize(page.GetTable((TableIndexEnum)i), textEncoding);
            var bytes = result.Buffer.Slice(0, result.Length);
            buffer.Write(bytes);
        }
        return (Buffer: buffer, Length: buffer.Position);
    }

    public Page Hydrate(IBuffer buffer, ITableSerializer serializer, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        var page = new Page(serializer);
        buffer.Position = 0;

        var result0 = serializer.Deserialize(buffer, textEncoding);
        page.SetTable(0, result0.Table);
        page.SetBuffer(0, result0.RowBuffers);

        // loop for each addition table in page
        for (int i = 1; i < page.TableCount; i++)
        {
            var result = serializer.Deserialize(buffer, textEncoding);
            page.SetTable((TableIndexEnum)i, result.Table);
            page.SetBuffer((TableIndexEnum)i, result.RowBuffers);
        }
        return page;
    }
}