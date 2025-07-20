namespace Dbarone.Net.Database;
using Dbarone.Net.Assertions;
using Dbarone.Net.Document;

/// <summary>
/// Base class for all pages.
/// </summary>
public class Page
{
    /// <summary>
    /// Header is the first/only row on the first data table.
    /// </summary>
    public TableRow Header => this.Data[0][0];

    #region Standard Header Fields

    public long PageId
    {
        get
        {
            return Header["PI"].AsInteger;
        }
        set
        {
            Header["PI"] = value;
        }
    }

    public long TableCount
    {
        get
        {
            return Header["TC"].AsInteger;
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

    public bool IsLeaf { get; set; } = false;   // only used for b-tree

    /// <summary>
    /// Size of data table serialised at point of reading the page in.
    /// </summary>
    public int DataLength { get; set; }

    /// <summary>
    /// Page data. Includes the header table. Must be at least 1 data table per page.
    /// </summary>
    public List<Table> Data { get; set; } = new List<Table>();

    /// <summary>
    /// Stores the byte arrays for each:
    /// - Table (dimension #1)
    /// - Row (dimension #2)
    /// </summary>
    public List<List<byte[]>> Buffers { get; set; } = new List<List<byte[]>>();

    #endregion

    public Page() { }

    public Page(int pageId, PageType pageType)
    {
        this.PageId = pageId;
        this.PageType = pageType;
    }

    /// <summary>
    /// Sets the current page to be an empty page.
    /// </summary>
    public void Clear()
    {
        this.PageType = PageType.Empty;
        this.IsDirty = true;
    }
}