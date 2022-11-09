using Dbarone.Net.Database;
using System.Linq;
using Dbarone.Net.Assertions;

public class HeapTableManager<TRow, TPageType> : IHeapTableManager<TRow> where TPageType : Page
{
    private int? ParentObjectId { get; set; }
    private BufferManager BufferManager { get; set; }
    IEnumerable<ColumnInfo> Columns { get; set; } = default!;
    private int TailPageId { get; set; }

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
            this.TailPageId = row.DataPageId;
        }
        else if (typeof(TPageType) == typeof(SystemTablePage))
        {
            this.TailPageId = 1;
        }
        else if (typeof(TPageType) == typeof(SystemColumnPage))
        {
            var tablesHeap = new HeapTableManager<SystemTablePageData, SystemTablePage>(bufferManager);
            var loc = tablesHeap.SearchSingle(d => d.ObjectId == parentObjectId);
            var row = tablesHeap.GetRow(loc);
            this.TailPageId = row.ColumnPageId;
        }
        else
        {
            throw new Exception("Unexpected data type for new HeapTableManager instance.");
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
        var page = this.BufferManager.GetPage<TPageType>(this.TailPageId);
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
                page = this.BufferManager.GetPage<TPageType>(TailPageId);
            }
            else
            {
                int? prevPageId = page.Headers().PrevPageId;
                if (prevPageId != null)
                {
                    page = this.BufferManager.GetPage<TPageType>(prevPageId.Value);
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
        int? prevId = TailPageId;
        do
        {
            var page = this.BufferManager.GetPage<TPageType>(prevId.Value);
            for (ushort i = 0; i < page.Headers().SlotsUsed; i++)
            {
                if (!page.GetRowStatus(i).HasFlag(RowStatus.Deleted))
                {
                    var row = page.GetRowAtSlot(i);
                    if (page.GetRowStatus(i).HasFlag(RowStatus.Overflow))
                    {
                        // data is in overflow
                        // TODO: For time being, assume all overflow data comes from data pages, so is Dictionary data.
                        var buffer = GetOverflowData((row as OverflowPointer)!);
                        row = (IPageData)new DictionaryPageData(Serializer.DeserializeDictionary(this.Columns, buffer).Result!);
                    }
                    if (predicate((TRow)row))
                    {
                        return new DataRowLocation(page.Headers().PageId, i);
                    }
                }
            }
            prevId = page.Headers().PrevPageId;
        }
        while (prevId != null);
        return null;
    }

    public DataRowLocation[] SearchMany(Func<TRow, bool> predicate)
    {
        List<DataRowLocation> locations = new List<DataRowLocation>();
        int? prevId = TailPageId;
        do
        {
            var page = this.BufferManager.GetPage<TPageType>(prevId.Value);
            for (ushort i = 0; i < page.Headers().SlotsUsed; i++)
            {
                if (!page.GetRowStatus(i).HasFlag(RowStatus.Deleted))
                {
                    var row = page.GetRowAtSlot(i);
                    if (page.GetRowStatus(i).HasFlag(RowStatus.Overflow))
                    {
                        // data is in overflow
                        // TODO: For time being, assume all overflow data comes from data pages, so is Dictionary data.
                        var buffer = GetOverflowData((row as OverflowPointer)!);
                        row = (IPageData)new DictionaryPageData(Serializer.DeserializeDictionary(this.Columns, buffer).Result!);
                    }
                    if (predicate((TRow)row))
                    {
                        locations.Add(new DataRowLocation(page.Headers().PageId, i));
                    }
                }
            }
            prevId = page.Headers().PrevPageId;
        }
        while (prevId != null);
        return locations.ToArray();
    }

    public TRow GetRow(DataRowLocation location)
    {
        var page = this.BufferManager.GetPage<TPageType>(location.PageId);
        return (TRow)page.GetRowAtSlot(location.Slot);
    }

    /// <summary>
    /// Heap is arranged in singly-linked list. When adding new pages, we insert at the start of the linked list.
    /// </summary>
    /// <param name="firstPage"></param>
    /// <exception cref="Exception"></exception>
    private void SetTailPage(Page page)
    {
        //page.Headers().PrevPageId = this.TailPageId;
        this.TailPageId = page.Headers().PageId;

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
                DataPageId = page.Headers().PageId,
                ColumnPageId = r.ColumnPageId
            }, r => r.ObjectId == this.ParentObjectId);
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
                DataPageId = r.DataPageId,
                ColumnPageId = page.Headers().PageId
            }, r => r.ObjectId == this.ParentObjectId);
        }
        else
        {
            throw new Exception("Unexpeected data type for new HeapTableManager instance.");
        }
    }

    /// <summary>
    /// Checks whether the row requires overflow processing. Processing steps are:
    /// 1. Remove existing overflow pages if current row uses overflow pages.
    /// 2. Checks whether new buffer.Length > page.GetOverflowThresholdSize()
    /// 3. If #2 is yes, then:
    /// 3a. Writes the LOB to new overflow pages
    /// 3b. Creates a new OverflowPointer object that references the overflow data.
    /// 3c. Returns the overflow object, serialised as a buffer.
    /// 4. If #2 is no then returns null
    /// </summary>
    /// <param name="row">The row to check check/process overflow logic for.</param>
    /// <returns>Returns an OverflowPointer record if overflow required. Otherwise, returns null.</returns>
    private OverflowPointer? ProcessOverflowIfRequired(TRow row, byte[] buffer, DataRowLocation? existingLocation)
    {
        // Get last page (insert) or existing page (update)
        var page = this.BufferManager.GetPage<TPageType>(existingLocation != null ? existingLocation.PageId : this.TailPageId);

        // Clear existing overflow data if present
        if (existingLocation != null)
        {
            var existingRowAsOverflowPointer = page.GetRowAtSlot(existingLocation.Slot) as OverflowPointer;
            if (existingRowAsOverflowPointer != null)
            {
                FreeOverflow(existingRowAsOverflowPointer.FirstOverflowPageId);
            }
        }

        if (buffer.Length > page.GetOverflowThresholdSize())
        {
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
            nextId = page.Headers().NextPageId;
            this.BufferManager.MarkFree(page);
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
        var page = this.BufferManager.GetPage<TPageType>(existingLocation != null ? existingLocation.PageId : this.TailPageId);
        var tail = this.BufferManager.GetPage<TPageType>(this.TailPageId);   // TODO: avoid creating page here - just use TailId?
        var buffer = this.BufferManager.SerialiseRow(_row!, RowStatus.None, this.Columns);
        bool isOverflowPointer = false;

        // Overflow processing 
        var overflowPointer = ProcessOverflowIfRequired(row, buffer, existingLocation);
        if (overflowPointer != null)
        {
            isOverflowPointer = true;
            _row = overflowPointer;
            buffer = this.BufferManager.SerialiseRow(_row, RowStatus.Overflow, Serializer.GetColumnsForType(typeof(OverflowPointer)));
        }

        if (existingLocation != null)
        {
            // update
            // Room on page? - if not, create new page.
            if (buffer.Length > page.GetAvailableSpaceForSlot(existingLocation.Slot))
            {
                page.SetRowStatus(existingLocation.Slot, RowStatus.Deleted);
                page = this.BufferManager.CreatePage<TPageType>(page.Headers().ParentObjectId, tail);
                this.SetTailPage(page);
                page.AddDataRow(_row!, buffer, isOverflowPointer);
            }
            else
            {
                page.UpdateDataRow(existingLocation.Slot, _row!, buffer, isOverflowPointer);
            }
        }
        else
        {
            // insert
            // Room on page? - if not, create new page.
            if (buffer.Length > page.GetFreeRowSpace())
            {
                page = this.BufferManager.CreatePage<TPageType>(page.Headers().ParentObjectId, page);
                this.SetTailPage(page);
            }
            page.AddDataRow(_row!, buffer, isOverflowPointer);
        }
    }

    public int AddRows(TRow[] rows)
    {
        foreach (var row in rows)
        {
            AddRow(row);
        }
        return rows.Length;
    }

    public int AddRow(TRow row)
    {
        UpsertRow(row, null);
        return 1;
    }

    public int UpdateRows(Func<TRow, TRow> transform, Func<TRow, bool> predicate)
    {
        var locations = SearchMany(predicate);
        foreach (var location in locations)
        {
            var page = this.BufferManager.GetPage<TPageType>(location.PageId);
            var currentStatus = page.Statuses[location.Slot];
            var currentRow = page.GetRowAtSlot(location.Slot);
            if (page.GetRowStatus(location.Slot).HasFlag(RowStatus.Overflow))
            {
                var overflowPointer = currentRow as OverflowPointer;
                currentRow = new DictionaryPageData(Serializer.DeserializeDictionary(this.Columns, GetOverflowData(overflowPointer!)).Result!);
            }
            var updatedRow = transform((TRow)currentRow)!;
            this.UpsertRow(updatedRow, location);
        }
        return locations.Length;
    }

    public int DeleteRows(Func<TRow, bool> predicate)
    {
        int rowsAffected = 0;
        TPageType? page = null;
        do
        {
            if (page == null)
            {
                page = this.BufferManager.GetPage<TPageType>(TailPageId);
            }
            else
            {
                int? prevPageId = page.Headers().PrevPageId;
                if (prevPageId != null)
                {
                    page = this.BufferManager.GetPage<TPageType>(prevPageId.Value);
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