using Dbarone.Net.Database;
using System.Linq;

public class HeapTableManager<TRow, TPageType> : IHeapTableManager<TRow> where TPageType : Page
{
    private BufferManager BufferManager { get; set; }
    IEnumerable<ColumnInfo> Columns { get; set; } = default!;
    private int FirstPageId { get; set; }

    public HeapTableManager(int firstPageId, BufferManager bufferManager)
    {
        this.BufferManager = bufferManager;
        this.FirstPageId = firstPageId;
    }

    public int Count()
    {
        return Scan().Count();
    }

    public TRow[] Scan()
    {
        var page = this.BufferManager.GetPage<TPageType>(FirstPageId);
        var data = page.Data().Select(r => (TRow)r);
        while (page.Headers().NextPageId != null)
        {
            page = this.BufferManager.GetPage<TPageType>(page.Headers().NextPageId!.Value);
            data = data.Union(page.Data().Select(r => (TRow)r));
        }
        return data.ToArray();
    }

    public DataRowLocation? SearchSingle(Func<TRow, bool> predicate)
    {
        int? nextId = FirstPageId;
        do
        {
            var page = this.BufferManager.GetPage<TPageType>(FirstPageId);
            for (var i = 0; i < page.Headers().SlotsUsed; i++)
            {
                var row = (TRow)page.GetRowAtSlot(i);
                if (predicate(row))
                {
                    return new DataRowLocation(page.Headers().PageId, i);
                }
            }
            nextId = page.Headers().NextPageId;
        }
        while (nextId != null);
        return null;
    }

    public DataRowLocation[] SearchMany(Func<TRow, bool> predicate)
    {
        List<DataRowLocation> locations = new List<DataRowLocation>();
        int? nextId = FirstPageId;
        do
        {
            var page = this.BufferManager.GetPage<TPageType>(FirstPageId);
            for (var i = 0; i < page.Headers().SlotsUsed; i++)
            {
                var row = (TRow)page.GetRowAtSlot(i);
                if (predicate(row))
                {
                    locations.Add(new DataRowLocation(page.Headers().PageId, i));
                }
            }
            nextId = page.Headers().NextPageId;
        }
        while (nextId != null);
        return locations.ToArray();
    }

    public TRow GetRow(DataRowLocation location){
        throw new NotSupportedException();
    }

    public void AddRow(TRow row){
        throw new NotSupportedException();
    }

    public void AddRows(TRow[] row){
        throw new NotSupportedException();
    }

    public void UpdateRows(Func<TRow, TRow> transform, Func<TRow, bool> predicate){
        throw new NotSupportedException();
    }

    public void DeleteRows(Func<TRow, bool> predicate){
        throw new NotSupportedException();
    }
}