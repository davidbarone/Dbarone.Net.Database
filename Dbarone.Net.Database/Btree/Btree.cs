using Dbarone.Net.Database;

/// <summary>
/// Implementation of B-Tree algorithm.
/// 
/// https://www.geeksforgeeks.org/dsa/introduction-of-b-tree-2/
/// 
/// </summary>
/// <remarks>
/// Properties:
/// 0. 3 types of node:
///     - root node - top level node in tree
///     - internal node - any node not root or leaf
///     - leaf node - contains actual data
/// 1. Order 'm' defines max number of children per page
/// 2. Order 'm' node can contain max m-1 keys
/// 3. All leaf nodes at same level / depth
/// 4. Keys of each node in B-Tree should be stored in ascending order
/// 5. All leaf nodes (except root) must have at least m/2 children
/// 6. All nodes except root node should have at least m/2-1 keys
/// 7. If root node is also leaf node (i.e. only node in tree), then no children and will have at least 1 key
/// 8. If root node is non-leaf node then will have at least 2 children and at least 1 key
/// 9. A non leaf node with n-1 key values should have n NON NULL children
/// Variations in this implementation
/// 1. Order not fixed - limited by space on the page
/// 
/// Struture of leaf page:
/// Data is in table area
/// 
/// Structure of interior page:
/// Table contains 
/// </remarks>

public class Btree
{
    public IBufferManager BufferManager { get; set; }
    public Page? Root { get; set; } = null;
    public string KeyColumn => Schema.Attributes.First(a => a.IsKey).AttributeName;
    public TableSchema Schema { get; set; }

    public Btree(IBufferManager bufferManager, TableSchema schema)
    {
        this.BufferManager = bufferManager;
        this.Schema = schema;
    }


    public void Search()
    {
    }

    /// <summary>
    /// Traverses a tree
    /// </summary>
    public TState Traverse<TState>(TState startingState, Func<TState, TableRow, TState> traverseFunction)
    {
        if (Root is not null)
        {
            return TraverseNode(Root, startingState, traverseFunction);
        }
        throw new Exception("whoops");
    }

    /// <summary>
    /// Inserts a new item into the tree
    /// </summary>
    /// <param name="row"></param>
    public void Insert(TableRow row)
    {
        if (Root == null)
        {
            Root = BufferManager.Create();
            Root.SetHeader("LEAF", true);
            Root.InsertCell(1, 0, row);
        }
        else
        {
            InsertNonFull(Root, row);
        }
    }

    public void Delete()
    {

    }

    private void InsertNonFull(Page node, TableRow row)
    {
        int i = node.Data[1].Count - 1;

        if (node.GetHeader("LEAF").AsBoolean == true)
        {
            while (i > 0 && GetKeyValue(row) < GetKeyValue(node.Data[1][i]))
            {
                node.Data[1][i + 1] = node.Data[1][i];
                i--;
            }
            node.Data[1][i + 1] = row;
        }
    }

    private TableCell GetKeyValue(TableRow row)
    {
        return row[KeyColumn];
    }

    private void SplitChild(Page parent, int index, Page child)
    {
    }

    /// <summary>
    /// Returns true if page storage is less than 50%
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private bool IsUnderflow(Page node)
    {
        var pageSize = this.BufferManager.PageSize;
        var used = node.GetPageSize();
        var free = pageSize - used;
        return free / pageSize < 0.5 ? true : false;
    }

    /// <summary>
    /// Returns true if page storage exceeds limit.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private bool IsOverflow(Page node)
    {
        var pageSize = this.BufferManager.PageSize;
        var used = node.GetPageSize();
        var free = pageSize - used;
        return free < 0 ? true : false;
    }

    private TState TraverseNode<TState>(Page node, TState startingState, Func<TState, TableRow, TState> traverseFunction)
    {
        int i;
        for (i = 0; i < node.Data[1].Count(); i++)
        {
            if (node.GetHeader("LEAF").AsBoolean == true)
            {
                startingState = traverseFunction(startingState, node.Data[1][i]);
            }
            else
            {
            }
        }
        return startingState;
    }
}