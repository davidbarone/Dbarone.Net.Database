namespace Dbarone.Net.Database;

using Dbarone.Net.Assertions;
using Dbarone.Net.Document;

/// <summary>
/// Base class for all pages.
/// </summary>
public class Page
{
    #region Fields / Properties

    private ITableSerializer TableSerializer { get; set; }

    /// <summary>
    /// Page data. Includes the header table. Must be at least 1 data table per page.
    /// </summary>
    private List<Table> Data { get; set; } = new List<Table>();

    /// <summary>
    /// Stores the byte arrays for each:
    /// - Dimension 1 = Table
    /// - Dimension 2 = Row
    /// </summary>
    private List<List<byte[]>> Buffers { get; set; } = new List<List<byte[]>>();

    #endregion

    #region Constructors

    public Page(ITableSerializer tableSerializer)
    {
        this.TableSerializer = tableSerializer;
    }

    public Page(ITableSerializer tableSerializer, int pageId)
    {
        this.TableSerializer = tableSerializer;
        InitialiseHeader(pageId);
    }

    public Page(ITableSerializer tableSerializer, int pageId, PageType pageType)
    {
        this.TableSerializer = tableSerializer;
        this.PageId = pageId;
        this.PageType = pageType;
    }

    #endregion

    #region Standard Header Fields

    /// <summary>
    /// Provides general access to the header table. Note the header is a single row.
    /// </summary>
    public TableRow Header => this.Data[0][0];

    public int PageId
    {
        get
        {
            return (int)Header["PI"].AsInteger;
        }
        set
        {
            Header["PI"] = value;
        }
    }

    public int TableCount
    {
        get
        {
            return (int)Header["TC"].AsInteger;
        }
        set
        {
            Header["TC"] = value;
        }
    }

    public PageType PageType
    {
        get
        {
            return (PageType)Header["PT"].AsInteger;
        }
        set
        {
            Header["PT"] = (long)value;
        }
    }

    public int? PrevPageId
    {
        get
        {
            return Header["PP"].Type == DocumentType.Null ? null : (int?)Header["PP"].AsInteger;
        }
        set
        {
            Header["PP"] = value;
        }
    }

    public int? NextPageId
    {
        get
        {
            return Header["NP"].Type == DocumentType.Null ? null : (int?)Header["NP"].AsInteger;
        }
        set
        {
            Header["NP"] = value;
        }
    }

    public int? ParentPageId
    {
        get
        {
            return Header["RP"].Type == DocumentType.Null ? null : (int?)Header["RP"].AsInteger;
        }
        set
        {
            Header["RP"] = value;
        }
    }

    public bool IsDirty
    {
        get
        {
            return Header["ID"].AsBoolean;
        }
        set
        {
            Header["ID"] = (bool)value;
        }
    }

    public TableCell GetHeader(string name)
    {
        return Header[name];
    }

    public void SetHeader(string name, object value)
    {
        Header[name] = new TableCell(value);
    }

    #endregion

    #region Table / Row Methods

    public Table GetTable(TableIndexEnum tableIndex)
    {
        return this.Data[(int)tableIndex];
    }

    public void SetTable(TableIndexEnum tableIndex, Table table)
    {
        // this version to be used to set a row when you also have the buffer information
        // used internally to move rows + buffers between pages, for example during
        // btree merge operation or borrow operation
        if (tableIndex < 0)
        {
            throw new Exception("Invalid table index");
        }
        if ((int)tableIndex >= this.Data.Count())
        {
            // insert table
            this.Data.Insert((int)tableIndex, table);
        }
        else
        {
            this.Data[(int)tableIndex] = table;
        }

        // update table count header if NOT writing the header table
        if (tableIndex != (int)TableIndexEnum.HEADER)
        {
            this.TableCount = this.Data.Count();
        }
    }

    public byte[] GetBuffer(TableIndexEnum tableIndex, int rowIndex)
    {
        return this.Buffers[(int)tableIndex][rowIndex];
    }

    public void SetBuffer(TableIndexEnum tableIndex, List<byte[]> buffer)
    {
        // this version to be used to set a row when you also have the buffer information
        // used internally to move rows + buffers between pages, for example during
        // btree merge operation or borrow operation
        if (tableIndex < 0)
        {
            throw new Exception("Invalid table index");
        }
        if ((int)tableIndex >= this.Buffers.Count())
        {
            // insert table
            this.Buffers.Insert((int)tableIndex, buffer);
        }
        else
        {
            this.Buffers[(int)tableIndex] = buffer;
        }

        // update table count header if NOT writing the header table
        if (tableIndex != (int)TableIndexEnum.HEADER)
        {
            this.TableCount = this.Data.Count();
        }
    }

    public TableRow GetRow(TableIndexEnum tableIndex, int rowIndex)
    {
        return this.Data[(int)tableIndex][rowIndex];
    }

    public void SetRow(TableIndexEnum tableIndex, int rowIndex, TableRow row, byte[] bytes)
    {
        // this version to be used to set a row when you also have the buffer information
        // used internally to move rows + buffers between pages, for example during
        // btree merge operation or borrow operation
        if (tableIndex < 0)
        {
            throw new Exception("Invalid table index");
        }
        while ((int)tableIndex >= this.Data.Count())
        {
            this.Data.Insert(this.Data.Count(), new Table());
        }
        while ((int)tableIndex >= this.Buffers.Count())
        {
            this.Buffers.Insert(this.Buffers.Count(), new List<byte[]>());
        }
        if (rowIndex >= 0 && rowIndex < this.GetTable(tableIndex).Count())
        {
            // index exists - update
            this.Data[(int)tableIndex][rowIndex] = row;
            this.Buffers[(int)tableIndex][rowIndex] = bytes;
        }
        else
        {
            // doesn't exist - insert
            this.Data[(int)tableIndex].Insert(rowIndex, row);
            this.Buffers[(int)tableIndex].Insert(rowIndex, bytes);
        }

        // update table count header
        this.TableCount = this.Data.Count();
    }

    /// <summary>
    /// Sets a data row in the page. Additionally, the byte array for the row is cached for checkpointing the page.
    /// </summary>
    /// <param name="tableIndex"></param>
    /// <param name="rowIndex"></param>
    /// <param name="row"></param>
    /// <exception cref="Exception"></exception>
    public void SetRow(TableIndexEnum tableIndex, int rowIndex, TableRow row)
    {
        var bytes = TableSerializer.SerializeRow(row).Buffer.ToArray();
        SetRow(tableIndex, rowIndex, row, bytes);
    }

    public void DeleteRow(TableIndexEnum tableIndex, int rowIndex)
    {
        if (tableIndex < 0 || (int)tableIndex >= this.Data.Count())
        {
            throw new Exception("Invalid table index");
        }
        if (rowIndex <= 0 || rowIndex >= this.GetTable(tableIndex).Count())
        {
            throw new Exception("Invalid rowIndex");
        }
        this.Data[(int)tableIndex].RemoveAt(rowIndex);
        this.Buffers[(int)tableIndex].RemoveAt(rowIndex);
    }

    /// <summary>
    /// Inserts a row into a table at the nth position, pushing following items right by one.
    /// </summary>
    /// <param name="tableIndex"></param>
    /// <param name="index"></param>
    /// <param name="row"></param>
    public void InsertRow(TableIndexEnum tableIndex, int index, TableRow row, byte[] bytes)
    {
        // this version to be used to set a row when you also have the buffer information
        // used internally to move rows + buffers between pages, for example during
        // btree merge operation or borrow operation
        if (tableIndex < 0)
        {
            throw new Exception("Invalid table index");
        }
        while ((int)tableIndex >= this.Data.Count())
        {
            this.Data.Insert(this.Data.Count(), new Table());
        }
        while ((int)tableIndex >= this.Buffers.Count())
        {
            this.Buffers.Insert(this.Buffers.Count(), new List<byte[]>());
        }

        // insert data + buffer
        this.Data[(int)tableIndex].Insert(index, row);
        this.Buffers[(int)tableIndex].Insert(index, bytes);

        // update table count header
        this.TableCount = this.Data.Count();
    }

    /// <summary>
    /// Inserts a row into a table at the nth position, pushing following items right by one.
    /// </summary>
    /// <param name="tableIndex"></param>
    /// <param name="index"></param>
    /// <param name="row"></param>
    public void InsertRow(TableIndexEnum tableIndex, int index, TableRow row)
    {
        var bytes = TableSerializer.SerializeRow(row).Buffer.ToArray();
        InsertRow(tableIndex, index, row, bytes);
    }

    #endregion

    #region Other Methods

    /// <summary>
    /// Sets defaults for page
    /// </summary>
    public void InitialiseHeader(int pageId, bool dirty = true)
    {
        // Initialise header
        TableRow row = new TableRow();
        Table t = new Table(row);
        this.Data.Add(t);
        this.PageId = pageId;
        this.TableCount = 1;
        this.PageType = PageType.Empty;
        this.PrevPageId = null;
        this.NextPageId = null;
        this.ParentPageId = null;
        this.IsDirty = dirty;       // when initialising new page, defaults to dirty
    }

    /// <summary>
    /// Sets the current page to be an empty page.
    /// </summary>
    public void Empty()
    {
        this.PageType = PageType.Empty;
        this.IsDirty = true;
    }

    /// <summary>
    /// Calculates the number of used bytes in the page
    /// </summary>
    /// <returns></returns>
    public int GetPageSize()
    {
        int size = 0;

        for (int i = 0; i < this.TableCount; i++)
        {
            size += GetTableSize(i);
        }
        return size;
    }

    public int GetTableSize(int tableIndex)
    {
        // schema attribute
        var size = ZigZag.SizeOf(1);                   // there is 0/1 flag for schema at start of table

        // row count
        size += ZigZag.SizeOf(this.Data[tableIndex].Count);  // row count follows next

        // data
        size += this.Buffers[tableIndex].Sum(r => r.Length); // Then the row buffers

        return size;
    }

    #endregion
}