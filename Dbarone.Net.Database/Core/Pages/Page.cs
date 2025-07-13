namespace Dbarone.Net.Database;
using Dbarone.Net.Assertions;
using Dbarone.Net.Document;

/// <summary>
/// Base class for all pages.
/// </summary>
public class Page
{
    #region Header Fields

    public int PageId { get; set; }
    public PageType PageType { get; set; }
    public int? PrevPageId { get; set; }
    public int? NextPageId { get; set; }
    public int? ParentPageId { get; set; }
    public bool IsDirty { get; set; } = false;

    /// <summary>
    /// Size of data table serialised at point of reading the page in.
    /// </summary>
    public int DataLength { get; set; }

    /// <summary>
    /// Main data in page
    /// </summary>
    public Table Data { get; set; }

    public List<object> Cells { get; set; } = new List<object>();
    public byte[][] CellBuffers { get; set; } = new byte[0][];

    /// <summary>
    /// The metadata for the page.
    /// </summary>
    /// <remarks>
    /// This object is specific for each page type.
    /// </remarks>
    public object Header { get; set; }
    public byte[] HeaderBuffer { get; set; }

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