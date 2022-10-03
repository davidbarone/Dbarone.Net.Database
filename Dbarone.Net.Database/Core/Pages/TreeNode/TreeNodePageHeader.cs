namespace Dbarone.Net.Database;

public interface ITreeNodePageHeader : IPageHeader {
   /// <summary>
   /// The parent tree node. If not set, then the current node is a root node.
   /// </summary>
   int? ParentPageId { get; set; }
}

/// <summary>
/// Headers for tree node page type.
/// </summary>
public class TreeNodePageHeader : PageHeader, ITreeNodePageHeader
{
    public int? ParentPageId {get; set;}
}