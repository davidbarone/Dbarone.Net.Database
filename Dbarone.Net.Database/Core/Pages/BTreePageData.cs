using Dbarone.Net.Database;

namespace Dbarone.Net.Database;

/// <summary>
/// Represents a b-tree page node. 
/// </summary>
/// <remarks>
/// B-tree node can be either a leaf node or non-leaf (interior) node. Can also be either data node or index node.
/// </remarks>
public class BTreePageData
{
    /// <summary>
    /// Set to true for leaf node. Otherwise, is an internal node.
    /// </summary>
    private bool IsLeaf { get; set; }

    /// <summary>
    /// The level of the node. Level 0 nodes are leaf nodes. Higher levels are internal nodes.
    /// </summary>
    private int Level { get; set; }

    /// <summary>
    /// Set to true for index node. Otherwise is a data node.
    /// </summary>
    private bool IsIndex { get; set; }

    /// <summary>
    /// The order of the tree. Internal nodes can have maxiumum [Order - 1] keys.
    /// </summary>
    private int Order { get; set; }

    /// <summary>
    /// The number of cells on the page.
    /// </summary>
    private short Cells { get; set; }

    /// <summary>
    /// The keys for the node. 
    /// </summary>
    private IList<object> Keys { get; set; } = new List<object>();

    private IList<object> Children { get; set; } = new List<object>();

    public IList<RowStatus> Statuses { get; set; } = new List<RowStatus>();
}