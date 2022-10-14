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

    /// <summary>
    /// Scans through linked list to get last page.
    /// </summary>
    /// <returns></returns>
    private int GetLastPage()
    {
        var page = this.BufferManager.GetPage<TPageType>(FirstPageId);
        while (page.Headers().NextPageId != null)
        {
            page = this.BufferManager.GetPage<TPageType>(page.Headers().NextPageId!.Value);
        }
        return page.Headers().PageId;
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
            for (ushort i = 0; i < page.Headers().SlotsUsed; i++)
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
            for (ushort i = 0; i < page.Headers().SlotsUsed; i++)
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

    public TRow GetRow(DataRowLocation location)
    {
        throw new NotSupportedException();
    }

    public void AddRow(TRow row)
    {
        var pageId = GetLastPage();
        var page = this.BufferManager.GetPage<TPageType>(pageId);
        var columns = this.BufferManager.GetColumnsForPage(page);
        var buffer = this.BufferManager.SerialiseRow(row, columns);
        page.AddDataRow(row, buffer);
    }

    public void AddRows(TRow[] row)
    {
        throw new NotSupportedException();
    }

    public void UpdateRows(Func<TRow, TRow> transform, Func<TRow, bool> predicate)
    {
        var locations = SearchMany(predicate);
        foreach (var location in locations){
            var page = this.BufferManager.GetPage<TPageType>(location.PageId);
            var currentRowSize = page.GetAvailableSpaceForSlot(location.Slot);
            var updatedRow = transform((TRow)page.GetRowAtSlot(location.Slot))!;
            var buffer = this.BufferManager.SerialiseRow(updatedRow, this.BufferManager.GetColumnsForPage(page));
            page.UpdateDataRow(location.Slot, updatedRow, buffer);
        }
    }

    public void DeleteRows(Func<TRow, bool> predicate)
    {
        throw new NotSupportedException();
    }
}