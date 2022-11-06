using Dbarone.Net.Database;
using System.Linq;
using Dbarone.Net.Assertions;

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

        // Get column information
        this.Columns = GetColumnInformation();
    }

    private byte[] GetOverflowData(OverflowPointer overflowPointer)
    {
        var pageId = overflowPointer.FirstOverflowPageId;
        var page = this.BufferManager.GetPage<OverflowPage>(pageId);
        IEnumerable<byte> data = page.GetOverflowData();
        int? nextPageId = page.Headers().NextPageId;
        while (nextPageId != null)
        {
            page = this.BufferManager.GetPage<OverflowPage>(nextPageId.Value);
            data = data.Concat(page.GetOverflowData());
            nextPageId = page.Headers().NextPageId;
        }
        return data.ToArray();
    }

    private IEnumerable<ColumnInfo> GetColumnInformation()
    {
        var page = this.BufferManager.GetPage<TPageType>(this.LastPageId);
        var columns = this.BufferManager.GetColumnsForPage(page);
        return columns;
    }

    public int Count()
    {
        return Scan().Count();
    }

    public IEnumerable<TRow> Scan()
    {
        TPageType? page = null;
        do
        {
            if (page == null)
            {
                page = this.BufferManager.GetPage<TPageType>(FirstPageId);
            }
            else
            {
                int? nextPageId = page.Headers().NextPageId;
                if (nextPageId != null)
                {
                    page = this.BufferManager.GetPage<TPageType>(nextPageId.Value);
                }
                else
                {
                    page = null;
                }
            }
            if (page != null)
            {
                for (var i = 0; i < page.Headers().SlotsUsed; i++)
                {
                    var item = page._data[i];
                    var status = page.Statuses[i];
                    if (!status.HasFlag(RowStatus.Deleted))
                    {
                        var itemAsOverflowPointer = item as OverflowPointer;
                        if (itemAsOverflowPointer != null)
                        {
                            // overflow data
                            var overflowData = GetOverflowData(itemAsOverflowPointer);
                            var overflowObj = Serializer.DeserializeDictionary(this.Columns, overflowData);
                            var pd = new DictionaryPageData(overflowObj.Result);
                            yield return (TRow)(object)pd;
                        }
                        else
                        {
                            // normal data
                            yield return (TRow)item;
                        }
                    }
                }
            }
        } while (page != null);
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

    /// <summary>
    /// Checks whether the row requires overflow processing. Processing steps are:
    /// 1. Checks whether buffer.Length > page.GetOverflowThresholdSize()
    /// 2. If #1 is yes, then:
    /// 2a. Frees up all the existing overflow pages
    /// 2b. Writes the LOB to new overflow pages
    /// 2c. Creates a new OverflowPointer object that references the overflow data.
    /// 2d. Returns the overflow object, serialised as a buffer.
    /// 3. If #1 is no then returns null
    /// </summary>
    /// <param name="row">The row to check check/process overflow logic for.</param>
    /// <returns>Returns an OverflowPointer record if overflow required. Otherwise, returns null.</returns>
    private OverflowPointer? ProcessOverflowIfRequired(TRow row, byte[] buffer, DataRowLocation? existingLocation)
    {
        // Get last page (insert) or existing page (update)
        var page = this.BufferManager.GetPage<TPageType>(existingLocation != null ? existingLocation.PageId : this.LastPageId);
        if (buffer.Length > page.GetOverflowThresholdSize())
        {
            if (existingLocation != null)
            {
                FreeOverflow(existingLocation.PageId);
            }
            var pageId = AddOverflowData(buffer);
            return new OverflowPointer(pageId, buffer.Length);
        }
        else
        {
            return null;
        }
    }

    private void FreeOverflow(int firstOverflowPageId)
    {
        int? nextId = firstOverflowPageId;
        while (nextId != null)
        {
            var page = this.BufferManager.GetPage(nextId.Value);
            page.MarkFree();
            nextId = page.Headers().NextPageId;
        }
    }

    /// <summary>
    /// Adds LOB data to overflow pages.
    /// </summary>
    /// <param name="bytes">Byte stream of overflow data.</param>
    /// <returns>The first page id of the overflow data.</returns>
    private int AddOverflowData(byte[] bytes)
    {
        var i = 0;
        BufferBase buffer = new BufferBase(bytes);
        var bufferLength = buffer.Size;
        var remainder = bufferLength;
        int maxChunkSize;
        OverflowPage? overflow = null;
        OverflowPage? prevPage = null;
        int firstPageId = 0;

        do
        {
            overflow = this.BufferManager.CreatePage<OverflowPage>();

            if (prevPage == null)
            {
                // On first iteration, save pointer in main page.
                firstPageId = overflow.Headers().PageId;
            }
            else
            {
                // For non-first iteration, set up doubly linked list to previous page.
                prevPage.Headers().NextPageId = overflow.Headers().PageId;
                overflow.Headers().PrevPageId = prevPage.Headers().PageId;
            }
            prevPage = overflow;
            maxChunkSize = overflow.GetFreeRowSpace();
            var chunkSize = maxChunkSize > remainder ? remainder : maxChunkSize;
            var chunk = buffer.Slice(i, chunkSize);
            overflow.AddDataRow(new BufferPageData(chunk), chunk, false);
            i += chunkSize;
            remainder -= chunkSize;
        }
        while (remainder > 0);
        return firstPageId;
    }

    private void UpsertRow(TRow row, DataRowLocation? existingLocation)
    {
        object _row = row;
        var page = this.BufferManager.GetPage<TPageType>(existingLocation!=null ? existingLocation.PageId : this.LastPageId);
        var buffer = this.BufferManager.SerialiseRow(_row!, RowStatus.None, this.Columns);
        bool isOverflowPointer = false;

        // Overflow processing 
        var overflowPointer = ProcessOverflowIfRequired(row, buffer, null);
        if (overflowPointer != null)
        {
            isOverflowPointer = true;
            _row = overflowPointer;
            buffer = this.BufferManager.SerialiseRow(_row, RowStatus.Overflow, Serializer.GetColumnsForType(typeof(OverflowPointer)));
        }

        if (existingLocation!=null){
            // update
            // Room on page? - if not, create new page.
            if (buffer.Length > page.GetAvailableSpaceForSlot(existingLocation.Slot))
            {
                page = this.BufferManager.CreatePage<TPageType>(page.Headers().ParentObjectId, page);
                // update last page ids
                UpdateLastPageId(page.Headers().PageId);
            }
            page.UpdateDataRow(existingLocation.Slot, _row!, buffer, isOverflowPointer);

        } else
        {
            // insert
            // Room on page? - if not, create new page.
            if (buffer.Length > page.GetFreeRowSpace())
            {
                page = this.BufferManager.CreatePage<TPageType>(page.Headers().ParentObjectId, page);

                // update last page ids
                UpdateLastPageId(page.Headers().PageId);
            }
            page.AddDataRow(_row!, buffer, isOverflowPointer);
        }
    }

    public void AddRows(TRow[] rows)
    {
        foreach (var row in rows)
        {
            AddRow(row);
        }
    }

    public void AddRow(TRow row) {
        UpsertRow(row, null);
    }

    public void UpdateRows(Func<TRow, TRow> transform, Func<TRow, bool> predicate)
    {
        var locations = SearchMany(predicate);
        foreach (var location in locations)
        {
            var page = this.BufferManager.GetPage<TPageType>(location.PageId);
            var currentStatus = page.Statuses[location.Slot];
            var updatedRow = transform((TRow)page.GetRowAtSlot(location.Slot))!;
            this.UpsertRow(updatedRow, location);
        }
    }

    public int DeleteRows(Func<TRow, bool> predicate)
    {
        int rowsAffected = 0;
        TPageType? page = null;
        do
        {
            if (page == null)
            {
                page = this.BufferManager.GetPage<TPageType>(FirstPageId);
            }
            else
            {
                int? nextPageId = page.Headers().NextPageId;
                if (nextPageId != null)
                {
                    page = this.BufferManager.GetPage<TPageType>(nextPageId.Value);
                }
                else
                {
                    page = null;
                }
            }
            if (page != null)
            {
                for (ushort i = 0; i < page.Headers().SlotsUsed; i++)
                {
                    var item = page._data[i];
                    var itemAsOverflowPointer = item as OverflowPointer;
                    if (itemAsOverflowPointer != null)
                    {
                        // overflow data
                        var overflowData = GetOverflowData(itemAsOverflowPointer);
                        var overflowObj = Serializer.DeserializeDictionary(this.Columns, overflowData);
                        var pd = new DictionaryPageData(overflowObj.Result);
                        if (predicate.Invoke((TRow)(object)pd))
                        {
                            page.SetRowStatus(i, RowStatus.Deleted);
                            rowsAffected++;
                        }
                    }
                    else
                    {
                        // normal data
                        if (predicate.Invoke((TRow)item))
                        {
                            page.SetRowStatus(i, RowStatus.Deleted);
                            rowsAffected++;
                        }
                    }
                }
            }
        } while (page != null);
        return rowsAffected;
    }
}