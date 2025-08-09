using Dbarone.Net.Database;

/// <summary>
/// LATEST CHANGE V2
/// Implementation of B-Tree algorithm.
/// https://www.geeksforgeeks.org/dsa/introduction-of-b-tree-2/
/// </summary>
/// <remarks>
/// Properties:
/// 0. 3 types of node:
///     - root node - top level node in tree
///     - internal node - any node not root or leaf
///     - leaf node - contains actual data
/// 1. Order (m) defines max number of children per page
/// 2. Order (m) node can contain max m-1 keys
/// 3. Degree (m/2) defines minimum number of children in non root node
/// 4. All leaf nodes at same level / depth
/// 5. Keys of each node in B-Tree should be stored in ascending order
/// 6. All leaf nodes (except root) must have at least m/2 children
/// 7. All nodes except root node should have at least m/2-1 keys
/// 8. If root node is also leaf node (i.e. only node in tree), then no children and will have at least 1 key
/// 9. If root node is non-leaf node then will have at least 2 children and at least 1 key
/// 10. A non leaf node with n-1 key values should have n NON NULL children
/// Variations in this implementation
/// 1. Order is optional. If not set, then min/max children is defined by space used / remaining on the page
/// 2. IsOverflow / IsUnderflow methods used to determine whether children / keys are outside permitted range
/// 3. Nodes are represented by Page object where:
///   - table[0] is header
///   - table[0]["LEAF"] defines whether node is leaf or not
///   - table[0].ParentPageId defines the parent node
///   - table[1] is array of keys (this applies to all pages)
///   - table[2] is array of children (this applies to non leaf pages only)
/// 4. Nodes are doubly-linked (can access parent via .ParentPageId and can access children via table[2])
/// </remarks>
public class Btree
{
    public IBufferManager BufferManager { get; set; }
    public Page? Root { get; set; } = null;
    public string KeyColumn => Schema.Attributes.First(a => a.IsKey).AttributeName;
    public TableSchema Schema { get; set; }

    /// <summary>
    /// Optional. If set, is the upper bound on number of children a node can have
    /// </summary>
    int? Order { get; set; }

    /// <summary>
    /// Optional. If set, is the lower bound of number of children a node can have.
    /// </summary>
    int? Degree => Order / 2;

    public Btree(IBufferManager bufferManager, TableSchema schema, int? order = null)
    {
        this.BufferManager = bufferManager;
        this.Schema = schema;
        this.Order = order;
    }


    public void Search()
    {
    }

    /// <summary>
    /// Traverses a tree
    /// </summary>
    public TState Traverse<TState>(TState startingState, Func<TState, Page, int, int, TState> traverseFunction)
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
            Root.SetRow(TableIndexEnum.BTREE_KEY, 0, row);
        }
        else
        {
            InsertNonFull(Root, row);
            if (IsOverflow(Root))
            {
                /*
                var newRoot = BufferManager.Create();
                newRoot.SetHeader("LEAF", true);
                TableRow child = new TableRow(new Dictionary<string, object>() { { "PID", Root.PageId } });
                newRoot.SetRow(TableIndexEnum.BTREE_CHILD, 0, child);
                Root.ParentPageId = newRoot.PageId;
                SplitChild(Root);

                // Get index to insert child
                int i = 0;
                if (GetKeyValue(newRoot.GetRow(TableIndexEnum.BTREE_KEY, i)) < GetKeyValue(row))
                {
                    i++;
                }

                var pid = newRoot.GetRow(TableIndexEnum.BTREE_CHILD, i)["PID"];
                var nodeToInsert = BufferManager.Get(pid);
                InsertNonFull(nodeToInsert, row);
                this.Root = newRoot;
                */
            }
        }
    }

    public void Delete()
    {

    }

    private void InsertNonFull(Page node, TableRow row)
    {
        // Note this method differs from classical 'InsertNonFull'
        // In classical version, node assumed to be not full at this point so no full check made
        // However, we need to check for full AFTER insert

        int i = node.GetTable(TableIndexEnum.BTREE_KEY).Count() - 1;

        if (node.GetHeader("LEAF").AsBoolean == true)
        {
            // Page is leaf page
            while (i >= 0 && GetKeyValue(node.GetRow(TableIndexEnum.BTREE_KEY, i)) > GetKeyValue(row))
            {
                node.SetRow(TableIndexEnum.BTREE_KEY, i + 1, node.GetRow(TableIndexEnum.BTREE_KEY, i));
                i--;
            }
            node.SetRow(TableIndexEnum.BTREE_KEY, i + 1, row);

            if (IsOverflow(node))
            {
                SplitChild(node);
            }
        }
        else
        {
            // Internal page
            while (i >= 0 && GetKeyValue(node.GetRow(TableIndexEnum.BTREE_KEY, i)) > GetKeyValue(row))
                i--;

            // get the child node to insert row into 
            int pid = (int)node.GetRow(TableIndexEnum.BTREE_CHILD, i + 1)["PID"].AsInteger;
            var childNodeToInsert = BufferManager.Get(pid);

            InsertNonFull(childNodeToInsert, row);

            if (IsOverflow(childNodeToInsert))
            {
                SplitChild(childNodeToInsert);
            }
        }
    }

    private TableCell GetKeyValue(TableRow row)
    {
        return row[KeyColumn];
    }

    private void SplitChild(Page child)
    {
        Page parentPage;
        int parentPageId = 0;
        int index;

        if (child.ParentPageId is not null)
        {
            // child page already has a parent
            parentPageId = child.ParentPageId.Value;
            parentPage = BufferManager.Get(parentPageId);
            index = new BinarySearch<int>(
                parentPage
                .GetTable(TableIndexEnum.BTREE_CHILD)
                .Select(r => (int)r["PID"].AsInteger).ToArray()
            ).Search(child.PageId);
        }
        else
        {
            // page hasn't got parent (must be root page)
            // create new root page with 1 child pointer
            // to current root page and update root.
            parentPage = BufferManager.Create();
            parentPageId = parentPage.PageId;
            parentPage.SetHeader("LEAF", false);
            index = 0;
            TableRow tr = new TableRow(new Dictionary<string, object> { { "PID", child.PageId } });
            parentPage.SetRow(TableIndexEnum.BTREE_CHILD, 0, tr);
            this.Root = parentPage;
        }

        // create new node to take 1/2 rows
        var newNode = BufferManager.Create();
        newNode.SetHeader("LEAF", child.GetHeader("LEAF").AsBoolean);

        // copy keys (all nodes)
        var degree = CalculateDegree(child);
        var order = child.GetTable(TableIndexEnum.BTREE_KEY).Count();

        for (int j = degree; j < order; j++)
        {
            newNode.SetRow(TableIndexEnum.BTREE_KEY, j - degree, child.GetRow(TableIndexEnum.BTREE_KEY, j));
        }

        // copy children (non-leaf nodes)
        if (!child.GetHeader("LEAF").AsBoolean)
        {
            order = child.GetTable(TableIndexEnum.BTREE_CHILD).Count();
            for (int j = degree; j < order; j++)
            {
                newNode.SetRow(TableIndexEnum.BTREE_CHILD, j - degree, child.GetRow(TableIndexEnum.BTREE_CHILD, j));
            }
        }

        // Make space in parent for new child in parent
        for (int j = parentPage.GetTable(TableIndexEnum.BTREE_KEY).Count(); j >= index + 1; j--)
        {
            parentPage.SetRow(TableIndexEnum.BTREE_CHILD, j + 1, parentPage.GetRow(TableIndexEnum.BTREE_CHILD, j));
        }

        // Add new child in parent
        TableRow r = new TableRow(new Dictionary<string, object> { { "PID", newNode.PageId } });
        parentPage.SetRow(TableIndexEnum.BTREE_CHILD, index + 1, r);

        for (int j = parentPage.GetTable(TableIndexEnum.BTREE_KEY).Count() - 1; j >= index; j--)
        {
            parentPage.SetRow(TableIndexEnum.BTREE_KEY, j + 1, parentPage.GetRow(TableIndexEnum.BTREE_KEY, j));
        }

        parentPage.SetRow(TableIndexEnum.BTREE_KEY, index, child.GetRow(TableIndexEnum.BTREE_KEY, degree - 1));

        // resize child node
        if (!child.GetHeader("LEAF").AsBoolean)
        {
            child.GetTable(TableIndexEnum.BTREE_KEY).Slice(0, degree - 1);
            child.GetTable(TableIndexEnum.BTREE_CHILD).Slice(0, degree);
        }
        else
        {
            child.GetTable(TableIndexEnum.BTREE_KEY).Slice(0, degree);
        }

        // set parent on both child pages
        child.ParentPageId = parentPageId;
        newNode.ParentPageId = parentPageId;

        // split parent recursively?
        if (IsOverflow(parentPage))
        {
            SplitChild(parentPage);
        }
    }

    /// <summary>
    /// Returns true if page storage is less than 50%
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private bool IsUnderflow(Page node)
    {
        if (Order is not null)
        {
            if (node.GetHeader("LEAF").AsBoolean == true)
            {
                return node.Data[1].Count < Order / 2;
            }
            else
            {
                return node.Data[1].Count < (int)(Order / 2) - 1;
            }
        }
        else
        {
            var pageSize = this.BufferManager.PageSize;
            var used = node.GetPageSize();
            var free = pageSize - used;
            return free / pageSize < 0.5 ? true : false;
        }
    }

    /// <summary>
    /// Returns true if page storage exceeds limit.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private bool IsOverflow(Page node)
    {
        if (Order is not null)
        {
            return node.Data[1].Count > Order;
        }
        else
        {
            var pageSize = this.BufferManager.PageSize;
            var used = node.GetPageSize();
            var free = pageSize - used;
            return free < 0 ? true : false;
        }
    }

    private int CalculateDegree(Page node)
    {
        if (this.Degree is not null)
        {
            // degree is explicitly specified
            return this.Degree.Value;
        }
        else
        {
            // calculate degree based on current page key count
            return (int)(node.GetTable(TableIndexEnum.BTREE_KEY).Count() + 1) / 2;
        }
    }

    private TState TraverseNode<TState>(Page node, TState startingState, Func<TState, Page, int, int, TState> traverseFunction, int nodeIndex = 0)
    {
        int i;
        int currentIndex = nodeIndex;
        for (i = 0; i < node.GetTable(TableIndexEnum.BTREE_KEY).Count; i++)
        {
            // traverse current node key
            startingState = traverseFunction(
                startingState,
                node,
                i,
                currentIndex);

            if (node.GetHeader("LEAF").AsBoolean == false)
            {
                // traverse ith child recursively
                int childId = (int)node.GetRow(TableIndexEnum.BTREE_CHILD, i)["PID"].AsInteger;
                var child = BufferManager.Get(childId);
                startingState = TraverseNode(child, startingState, traverseFunction, ++nodeIndex);
            }
        }
        if (node.GetHeader("LEAF").AsBoolean == false)
        {
            // traverse last child of non leaf nodes
            startingState = traverseFunction(
                startingState,
                node,
                i,
                currentIndex);

            int childId = (int)node.GetRow(TableIndexEnum.BTREE_CHILD, i)["PID"].AsInteger;
            var child = BufferManager.Get(childId);
            startingState = TraverseNode(child, startingState, traverseFunction, ++nodeIndex);
        }
        return startingState;
    }
}