using Dbarone.Net.Database;
using System.Linq;

public class HeapTableManager<TRow, TPageType> : IHeapTableManager<TRow> where TPageType : Page
{
    private int? ParentObjectId { get; set; }
    private BufferManager BufferManager { get; set; }
    IEnumerable<ColumnInfo> Columns { get; set; } = default!;
    private int FirstPageId { get; set; }
    private int LastPageId { get; set; }

    public HeapTableManager(BufferManager bufferManager, int? parentObjectId = null)
    {
        this.ParentObjectId = parentObjectId;
        this.BufferManager = bufferManager;

        // Calculate first/last page ids
        if (typeof(TPageType) == typeof(DataPage))
        {
            // Get from tableinfo
            var tablesHeap = new HeapTableManager<SystemTablePageData, SystemTablePage>(bufferManager);
            var loc = tablesHeap.SearchSingle(d => d.ObjectId == parentObjectId);
            var row = tablesHeap.GetRow(loc);
            this.FirstPageId = row.FirstDataPageId;
            this.LastPageId = row.LastDataPageId;
        }
        else if (typeof(TPageType) == typeof(SystemTablePage))
        {
            this.FirstPageId = 1;
            this.LastPageId = 1;
        }
        else if (typeof(TPageType) == typeof(SystemColumnPage))
        {
            var tablesHeap = new HeapTableManager<SystemTablePageData, SystemTablePage>(bufferManager);
            var loc = tablesHeap.SearchSingle(d => d.ObjectId == parentObjectId);
            var row = tablesHeap.GetRow(loc);
            this.FirstPageId = row.FirstColumnPageId;
            this.LastPageId = row.LastColumnPageId;
        }
        else
        {
            throw new Exception("Unexpeected data type for new HeapTableManager instance.");
        }

        // TO DO: Calculate columns here.

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
        var page = this.BufferManager.GetPage<TPageType>(location.PageId);
        return (TRow)page.GetRowAtSlot(location.Slot);
    }

    private void UpdateLastPageId(int lastPageId)
    {
        this.LastPageId = lastPageId;
        
        // Calculate first/last page ids
        if (typeof(TPageType) == typeof(DataPage))
        {
            // Get from tableinfo
            var tablesHeap = new HeapTableManager<SystemTablePageData, SystemTablePage>(this.BufferManager);
            tablesHeap.UpdateRows(r => new SystemTablePageData()
            {
                ObjectId = r.ObjectId,
                TableName = r.TableName,
                IsSystemTable = r.IsSystemTable,
                FirstDataPageId = r.FirstDataPageId,
                LastDataPageId = lastPageId,
                FirstColumnPageId = r.FirstColumnPageId,
                LastColumnPageId = r.LastColumnPageId
            }, r => r.ObjectId == this.ParentObjectId);
        }
        else if (typeof(TPageType) == typeof(SystemTablePage))
        {
            var bootPage = this.BufferManager.GetPage<BootPage>(0);
            bootPage.Headers().LastTablesPageId = lastPageId;
        }
        else if (typeof(TPageType) == typeof(SystemColumnPage))
        {
            // Get from tableinfo
            var tablesHeap = new HeapTableManager<SystemTablePageData, SystemTablePage>(this.BufferManager);
            tablesHeap.UpdateRows(r => new SystemTablePageData()
            {
                ObjectId = r.ObjectId,
                TableName = r.TableName,
                IsSystemTable = r.IsSystemTable,
                FirstDataPageId = r.FirstDataPageId,
                LastDataPageId = r.LastDataPageId,
                FirstColumnPageId = r.FirstColumnPageId,
                LastColumnPageId = lastPageId
            }, r => r.ObjectId == this.ParentObjectId);
        }
        else
        {
            throw new Exception("Unexpeected data type for new HeapTableManager instance.");
        }
    }

    public void AddRow(TRow row)
    {
        var page = this.BufferManager.GetPage<TPageType>(this.LastPageId);

        var columns = this.BufferManager.GetColumnsForPage(page);
        var buffer = this.BufferManager.SerialiseRow(row!, columns);

        // Room on page? - if not, create new page.
        if (buffer.Length > page.GetFreeRowSpace())
        {
            page = this.BufferManager.CreatePage<TPageType>(page.Headers().ParentObjectId, page);

            // update last page ids
            UpdateLastPageId(page.Headers().PageId);
        }

        page.AddDataRow(row!, buffer);
    }

    public void AddRows(TRow[] row)
    {
        throw new NotSupportedException();
    }

    public void UpdateRows(Func<TRow, TRow> transform, Func<TRow, bool> predicate)
    {
        var locations = SearchMany(predicate);
        foreach (var location in locations)
        {
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