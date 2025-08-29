using Dbarone.Net.Database;
using Dbarone.Net.Extensions;

/// <summary>
/// Implementation of B-Tree algorithm.
/// </summary>
/// <remarks>
/// Properties:
/// 0. 3 types of node:
///     - root node - top level node in tree
///     - internal node - any node not root or leaf - only stores keys, never full data values
///     - leaf node - contains actual data - only leaf nodes store full data values (as opposed to B-tree)
///
/// Structure of leaf node
/// ----------------------
/// 
///     k0 k1 k2 k3 k4 k5 ...
/// 
/// All keys in order
/// k5 > k4 > k3 > k2 > k1 > k0
/// 
/// Structure of non-leaf node
/// --------------------------
/// 
///      k0  k1  k2  k3
///    c0  c1  c2  c3  c4
/// 
/// c1 is a pointer to a child node
/// all keys in node pointed to by c1 are >= k0
/// k[0] in c1 == k0
/// 


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
/// 1. Only leaf nodes can store data. All other nodes only store key values. This approach similar to B+Tree
///    algorithm
/// 2. Order is optional. If not set, then min/max children is defined by space used / remaining on the page
/// 3. IsOverflow / IsUnderflow methods used to determine whether children / keys are outside permitted range
/// 4. Nodes are represented by Page object where:
///   - table[0] is header
///   - table[0]["LEAF"] defines whether node is leaf or not
///   - table[0].ParentPageId defines the parent node
///   - table[1] is array of keys (this applies to all pages)
///   - table[2] is array of children (this applies to non leaf pages only)
/// 5. Nodes are doubly-linked (can access parent via .ParentPageId and can access children via table[2])
/// 
/// Bibliography:
/// https://www.geeksforgeeks.org/dsa/introduction-of-b-tree-2/
/// https://www.geeksforgeeks.org/dsa/delete-operation-in-b-tree/
/// https://www.tutorialspoint.com/bplus-tree-deletion-in-data-structure
/// https://www.geeksforgeeks.org/dsa/deletion-in-b-tree/
/// https://www.geeksforgeeks.org/dsa/delete-operation-in-b-tree/
/// https://www.geeksforgeeks.org/dsa/introduction-of-b-tree-2/
/// https://www.programiz.com/dsa/deletion-from-a-b-plus-tree
/// </remarks>
public class Btree
{
    #region Properties

    public IBufferManager BufferManager { get; set; }
    public Page? Root { get; set; } = null;
    public string KeyColumn => Schema.Attributes.First(a => a.IsKey).AttributeName;
    public TableSchema Schema { get; set; }

    /// <summary>
    /// Optional. If set, is the upper bound on number of children a node can have
    /// </summary>
    private int? Order { get; set; }

    /// <summary>
    /// Optional. If set, is the lower bound of number of children a node can have.
    /// </summary>
    private int? Degree => Order / 2;

    #endregion

    #region Constructors

    public Btree(IBufferManager bufferManager, TableSchema schema, int? order = null)
    {
        this.BufferManager = bufferManager;
        this.Schema = schema;
        this.Order = order;
    }

    #endregion

    #region Public Methods

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
        }
    }

    public void Delete(TableCell key)
    {
        // Locate the key in leaf to delete
        var location = Search(key);
        if (location is not null)
        {
            // delete the key
            var node = BufferManager.Get(location.Value.PageId);
            node.DeleteRow(TableIndexEnum.BTREE_KEY, location.Value.RowIndex);

            // if first item in table, need to update internal nodes
            if (location.Value.RowIndex == 0)
            {
                this.ZeroKeyChangedUpdateParentKey(node);
            }

            // check underflow
            if (this.IsUnderflow(node))
            {
                ProcessUnderflow(node);
            }
        }


        // remove / replace references to deleted node

        // remove old root node and update new root if root node is empty

    }

    /// <summary>
    /// Searches for a key and returns the physical location of the record in the leaf node.
    /// </summary>
    /// <param name="key">Key to search</param>
    /// <param name="node">Current node to search</param>
    /// <returns>Physical location of key, or null if not found</returns>
    /// <exception cref="Exception">Throws error if ???</exception>
    public (int PageId, int RowIndex)? Search(TableCell key, Page node = null)
    {
        if (node is null && Root is null)
        {
            throw new Exception("Unable to search empty tree");
        }
        else if (node is null)
        {
            node = this.Root!;
        }

        int i = 0;
        int count = node.GetTable(TableIndexEnum.BTREE_KEY).Count();
        // Find the first key greater than or equal to the search key
        while (i < count && key > GetKeyValue(node.GetRow(TableIndexEnum.BTREE_KEY, i)))
        {
            i++;
        }

        // are we at the leaf?
        if (node.GetHeader("LEAF").AsBoolean)
        {
            if (i < count && key == GetKeyValue(node.GetRow(TableIndexEnum.BTREE_KEY, i)))
            {
                return (node.PageId, i);
            }
            else
            {
                return null;
            }
        }
        else
        {
            var cid = (int)node.GetRow(TableIndexEnum.BTREE_CHILD, i)["PID"].AsInteger;
            var c = BufferManager.Get(cid);
            return Search(key, c);
        }
    }

    #endregion

    #region Private Methods

    private void ProcessUnderflow(Page node)
    {
        if (IsRoot(node))
        {
            // do nothing - root allowed to underflow
        }
        else if (CanBorrowFromLeftSibling(node))
        {
            BorrowFromLeftSibling(node);
            ZeroKeyChangedUpdateParentKey(node);
        }
        else if (CanBorrowFromRightSibling(node))
        {
            BorrowFromRightSibling(node);
            var right = GetRightSibling(node);
            if (right is not null)
            {
                ZeroKeyChangedUpdateParentKey(right);
            }
        }
        else if (CanMergeWithLeftSibling(node))
        {
            var left = GetLeftSibling(node)!;
            Merge(left, node);
            RemoveParentKey(node);
            if (IsUnderflow(node))
            {
                ProcessUnderflow(node);
            }
        }
        else if (CanMergeWithRightSibling(node))
        {
            var right = GetRightSibling(node);
            if (right is not null)
            {
                Merge(node, right);
                RemoveParentKey(right);
                if (IsUnderflow(right))
                {
                    ProcessUnderflow(right);
                }
            }
        }
        else
        {
            // if got here, cannot fix underflow
            // this is ok ONLY if using dynamic order/degree
            // as when rows are variable width it is possible
            // for both siblings to not be able to offer a
            // record, but merging with either left or right
            // siblings would also overflow the current node
            if (this.Order is not null)
            {
                throw new Exception("Should not get here!");
            }

        }

        // If not first item, 

    }

    private void InsertNonFull(Page node, TableRow row)
    {
        // Note this method differs from classical 'InsertNonFull'
        // In classical version, node assumed to be not full at this point so no full check made
        // However, we need to check for full AFTER insert

        int i = node.GetTable(TableIndexEnum.BTREE_KEY).Count() - 1;

        if (node.GetHeader("LEAF").AsBoolean == true)
        {
            // leaf page
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
            // internal page
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

    /// <summary>
    /// Returns a TableRow object containing ONLY key value(s).
    /// Used to store keys in all non-leaf pages.
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
    private TableRow GetKeyAsTableRow(TableRow row)
    {
        TableRow tr = new TableRow();
        tr[KeyColumn] = GetKeyValue(row);
        return tr;
    }

    private (Page Parent, int ChildIndex) GetChildIndexInParent(Page child)
    {
        if (child.ParentPageId is not null)
        {
            var parentPageId = child.ParentPageId.Value;
            var parentPage = BufferManager.Get(parentPageId);

            var index = parentPage
                .GetTable(TableIndexEnum.BTREE_CHILD)
                .Select(r => (int)r["PID"].AsInteger)
                .ToList()
                .IndexOf(child.PageId);

            return (parentPage, index);
        }
        else
        {
            throw new Exception("child does not have parent");
        }
    }


    /// <summary>
    /// Splits a page into 2 child pages.
    /// 
    /// 1. If current page root, create new root with current page being 1st child of new root
    /// 2. Find parent child index of current page to split
    /// 3. Create new right child to take 1/2 records
    /// 4. Add right page into parent
    /// 5. Copy 1/2 keys from current to right node
    /// 6. If current is not leaf node, copy 1/2 children from current to right node
    /// 7. Repoint pages pointed to by moved children to new right page 
    /// 8. Insert new key / child in parent to point to new right page
    /// 9. Make sure current + right page both point to parent
    /// 10. Split parent if required (recursive)
    /// </summary>
    /// <param name="child">Page to split</param>
    /// <exception cref="Exception"></exception>
    private void SplitChild(Page child)
    {
        Page parentPage;
        int parentPageId = 0;
        int index;

        if (child.ParentPageId is not null)
        {
            (parentPage, index) = GetChildIndexInParent(child);
            parentPageId = parentPage.PageId;
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

        if (index < 0)
        {
            throw new Exception("Should not get here!");
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
        // children need parent repointed
        if (!child.GetHeader("LEAF").AsBoolean)
        {
            order = child.GetTable(TableIndexEnum.BTREE_CHILD).Count();
            for (int j = degree; j < order; j++)
            {
                newNode.SetRow(TableIndexEnum.BTREE_CHILD, j - degree, child.GetRow(TableIndexEnum.BTREE_CHILD, j));

                // update parent on child
                var cid = (int)child.GetRow(TableIndexEnum.BTREE_CHILD, j)["PID"].AsInteger;
                var c = BufferManager.Get(cid);
                c.ParentPageId = newNode.PageId;
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
                return node.GetTable(TableIndexEnum.BTREE_KEY).Count() < Order / 2;
            }
            else
            {
                return node.GetTable(TableIndexEnum.BTREE_KEY).Count() < (int)(Order / 2) - 1;
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
            return node.GetTable(TableIndexEnum.BTREE_KEY).Count() > Order;
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

    /// <summary>
    /// Gets a table row from a page given physical location
    /// </summary>
    /// <param name="pageId"></param>
    /// <param name="row"></param>
    /// <returns></returns>
    public TableRow? Get((int PageId, int RowIndex) location)
    {
        var page = BufferManager.Get(location.PageId);
        if (!page.GetHeader("LEAF").AsBoolean)
        {
            throw new Exception("Page is not a leaf page.");
        }
        var tr = page.GetRow(TableIndexEnum.BTREE_KEY, location.RowIndex);
        return tr;
    }

    private Page? GetRightSibling(Page node)
    {
        // gets next sibling page
        if (node.ParentPageId is null)
        {
            // root node does not have next page.
            return null;
        }
        else
        {
            int level = 0;
            var p = GetChildIndexInParent(node);
            while (p.ChildIndex == p.Parent.GetTable(TableIndexEnum.BTREE_CHILD).Count() - 1)
            {
                if (p.Parent.ParentPageId is null)
                {
                    // got up to parent - current page must be far right page - no next to return.
                    return null;
                }
                level++;
                p = GetChildIndexInParent(p.Parent);
            }
            var cid = (int)p.Parent.GetRow(TableIndexEnum.BTREE_CHILD, p.ChildIndex + 1)["PID"].AsInteger;
            var child = BufferManager.Get(cid);
            for (int i = 0; i < level; i++)
            {
                cid = (int)child.GetRow(TableIndexEnum.BTREE_CHILD, 0)["PID"].AsInteger;
                child = BufferManager.Get(cid);
            }
            return child;
        }
    }

    private Page? GetLeftSibling(Page node)
    {
        // gets next sibling page
        if (node.ParentPageId is null)
        {
            // root node does not have next page.
            return null;
        }
        else
        {
            int level = 0;
            var p = GetChildIndexInParent(node);
            while (p.ChildIndex == 0)
            {
                if (p.Parent.ParentPageId is null)
                {
                    // got up to parent - current page must be far left page - no prev to return.
                    return null;
                }
                level++;
                p = GetChildIndexInParent(p.Parent);
            }
            var cid = (int)p.Parent.GetRow(TableIndexEnum.BTREE_CHILD, p.ChildIndex - 1)["PID"].AsInteger;
            var child = BufferManager.Get(cid);
            for (int i = 0; i < level; i++)
            {
                var count = child.GetTable(TableIndexEnum.BTREE_CHILD).Count();
                cid = (int)child.GetRow(TableIndexEnum.BTREE_CHILD, count - 1)["PID"].AsInteger;
                child = BufferManager.Get(cid);
            }
            return child;
        }
    }

    /// <summary>
    /// This checks whether sibling can provide a record AND still not be in underflow state.
    /// Checking is similar (but opposite) to underflow checking.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private bool CanBorrowFromLeftSibling(Page node)
    {
        var left = GetLeftSibling(node);
        if (left is null)
        {
            return false;
        }
        else
        {
            if (Order is not null)
            {
                if (left.GetHeader("LEAF").AsBoolean == true)
                {
                    return left.GetTable(TableIndexEnum.BTREE_KEY).Count() > (int)Order / 2;
                }
                else
                {
                    return left.GetTable(TableIndexEnum.BTREE_KEY).Count() > (int)(Order / 2) - 1;
                }
            }
            else
            {
                throw new Exception("CanBorrowFromLeftSibling does not support dynamic underflow checking.");
            }
        }
    }

    /// <summary>
    /// This checks whether sibling can provide a record AND still not be in underflow state.
    /// Checking is similar (but opposite) to underflow checking.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public bool CanBorrowFromRightSibling(Page node)
    {
        var right = GetLeftSibling(node);
        if (right is null)
        {
            return false;
        }
        else
        {
            if (Order is not null)
            {
                if (right.GetHeader("LEAF").AsBoolean == true)
                {
                    return right.GetTable(TableIndexEnum.BTREE_KEY).Count() > (int)Order / 2;
                }
                else
                {
                    return right.GetTable(TableIndexEnum.BTREE_KEY).Count() > (int)(Order / 2) - 1;
                }
            }
            else
            {
                throw new Exception("CanBorrowFromLeftSibling does not support dynamic underflow checking.");
            }
        }
    }

    /// <summary>
    /// Borrows last key / child from left sibling.
    /// 
    ///                          pk0
    ///                       pc0   pc1
    ///                      /         \  
    ///                     /           \
    ///     lk0   lk1   lk2               rk0         
    ///  lc0   lc1   lc2   lc3         rc0   rc1
    /// 
    /// Steps:
    /// 1. if left is leaf node:
    /// 1a. move key (lk2) from left to 0th position on right, shifting right keys one place to right
    /// 1b  replace middle key (pk0) with KeyValue(lk2)
    /// 2. If left is not leaf node: 
    /// 2a. Take middle key (pk0) and insert as key0 on right
    /// 2b. Take last child from left (lc3) and insert at 0th on node shifting right children one place to right.
    /// 2c. Promote last key on left node (lk2) to new middle key (replacing pk0)
    /// </summary>
    /// <param name="node"></param>
    private void BorrowFromLeftSibling(Page node)
    {
        var left = GetLeftSibling(node);
        if (left is null)
        {
            throw new Exception("No left node to borrow from.");
        }
        var isLeaf = left.GetHeader("LEAF").AsBoolean;
        if (isLeaf)
        {
            var leftKeyCount = left.GetTable(TableIndexEnum.BTREE_KEY).Count();
            var keyToMove = left.GetRow(TableIndexEnum.BTREE_KEY, leftKeyCount - 1);
            var bufferToMove = left.GetBuffer(TableIndexEnum.BTREE_KEY, leftKeyCount - 1);
            node.InsertRow(TableIndexEnum.BTREE_KEY, 0, keyToMove, bufferToMove);
            left.DeleteRow(TableIndexEnum.BTREE_KEY, leftKeyCount - 1);
            var middle = GetMiddleKey(left, node);
            middle.Ancestor.SetRow(TableIndexEnum.BTREE_KEY, middle.KeyIndex, GetKeyAsTableRow(keyToMove));
        }
        else
        {
            var middleLoc = GetMiddleKey(left, node);
            var middleKey = middleLoc.Ancestor.GetRow(TableIndexEnum.BTREE_KEY, middleLoc.KeyIndex);
            var middleBuffer = middleLoc.Ancestor.GetBuffer(TableIndexEnum.BTREE_KEY, middleLoc.KeyIndex);
            node.InsertRow(TableIndexEnum.BTREE_KEY, 0, middleKey, middleBuffer);
            var leftKeyCount = left.GetTable(TableIndexEnum.BTREE_KEY).Count();
            var lastKeyLeft = left.GetRow(TableIndexEnum.BTREE_KEY, leftKeyCount - 1);
            var lastKeyBufferLeft = left.GetBuffer(TableIndexEnum.BTREE_KEY, leftKeyCount - 1);
            var lastChildLeft = left.GetRow(TableIndexEnum.BTREE_CHILD, leftKeyCount);
            var lastChildBufferLeft = left.GetBuffer(TableIndexEnum.BTREE_CHILD, leftKeyCount);
            // move last child to right node
            node.InsertRow(TableIndexEnum.BTREE_KEY, 0, lastChildLeft, lastChildBufferLeft);
            // move last key to middle
            middleLoc.Ancestor.SetRow(TableIndexEnum.BTREE_KEY, middleLoc.KeyIndex, lastKeyLeft, lastKeyBufferLeft);
            // delete last key + child from left
            left.DeleteRow(TableIndexEnum.BTREE_KEY, leftKeyCount - 1);
            left.DeleteRow(TableIndexEnum.BTREE_CHILD, leftKeyCount);
        }
    }

    /// <summary>
    /// Borrows first key / child from right sibling.
    /// 
    ///                     pk0
    ///                  pc0   pc1
    ///                 /         \  
    ///                /           \
    ///            lk0               rk0   rk1   rk2         
    ///         lc0   lc1         rc0   rc1   rc2   rc3
    /// 
    /// Steps:
    /// 1. if right is leaf node:
    /// 1a. move key (rk0) from right to nth/last position on left
    /// 1b  replace middle key (pk0) with KeyValue(rk1) (which is new 0th key on right)
    /// 2. If right is not leaf node: 
    /// 2a. Take middle key (pk0) and insert as key (lk1) on left
    /// 2b. Take first child from right (rc0) and insert at nth position on left/node (lc2).
    /// 2c. Promote first key on right node (rk0) to new middle key (replacing pk0)
    /// </summary>
    /// <param name="node"></param>
    private void BorrowFromRightSibling(Page node)
    {
        var right = GetRightSibling(node);
        if (right is null)
        {
            throw new Exception("No right node to borrow from.");
        }
        var isLeaf = right.GetHeader("LEAF").AsBoolean;
        if (isLeaf)
        {
            var leftKeyCount = node.GetTable(TableIndexEnum.BTREE_KEY).Count();
            var keyToMove = right.GetRow(TableIndexEnum.BTREE_KEY, 0);
            var bufferToMove = right.GetBuffer(TableIndexEnum.BTREE_KEY, 0);
            node.InsertRow(TableIndexEnum.BTREE_KEY, leftKeyCount, keyToMove, bufferToMove);
            right.DeleteRow(TableIndexEnum.BTREE_KEY, 0);
            var middle = GetMiddleKey(node, right);
            var newMiddleKey = right.GetRow(TableIndexEnum.BTREE_KEY, 0);
            middle.Ancestor.SetRow(TableIndexEnum.BTREE_KEY, middle.KeyIndex, GetKeyAsTableRow(keyToMove));
        }
        else
        {
            // move down middle key
            var leftKeyCount = node.GetTable(TableIndexEnum.BTREE_KEY).Count();
            var middleLoc = GetMiddleKey(node, right);
            var middleKey = middleLoc.Ancestor.GetRow(TableIndexEnum.BTREE_KEY, middleLoc.KeyIndex);
            var middleBuffer = middleLoc.Ancestor.GetBuffer(TableIndexEnum.BTREE_KEY, middleLoc.KeyIndex);
            node.InsertRow(TableIndexEnum.BTREE_KEY, leftKeyCount, middleKey, middleBuffer);

            // move right child[0] + promote right key[0]
            var firstKeyRight = right.GetRow(TableIndexEnum.BTREE_KEY, 0);
            var firstKeyBufferRight = right.GetBuffer(TableIndexEnum.BTREE_KEY, 0);
            var firstChildRight = right.GetRow(TableIndexEnum.BTREE_CHILD, 0);
            var firstChildBufferRight = right.GetBuffer(TableIndexEnum.BTREE_CHILD, 0);
            // move right first child to left node
            node.InsertRow(TableIndexEnum.BTREE_KEY, leftKeyCount, firstChildRight, firstChildBufferRight);
            // move right first key to middle
            middleLoc.Ancestor.SetRow(TableIndexEnum.BTREE_KEY, middleLoc.KeyIndex, firstKeyRight, firstKeyBufferRight);
            // delete first key + first child from right
            right.DeleteRow(TableIndexEnum.BTREE_KEY, 0);
            right.DeleteRow(TableIndexEnum.BTREE_CHILD, 0);
        }
    }

    private bool CanMergeWithLeftSibling(Page node)
    {
        return false;
    }

    private bool CanMergeWithRightSibling(Page node)
    {
        return false;
    }


    /// <summary>
    /// Gets the key between 2 pages.
    /// 
    /// Pages are linked to parent / interior pages through keys and child pointers
    /// For all non leaf pages, if there are n children, there are n-1 keys
    /// Data is assigned to left or right side of the key using formula:
    /// 
    /// if data less than key then left else right
    /// 
    /// 2 adjacent pages are linked via a middle key. Any data greater / equal to the
    /// middle key goes to the right page, else the left page.
    /// 
    ///                       pk0
    ///                    pc0   pc1
    ///                     /     \
    ///                    /       \
    ///                  lk0       rk0 
    ///               lc0   lc1 rc0   rc1
    /// 
    /// In above example, the middle key between pages l and r is pk0.
    /// 
    /// Sometimes, the middle key is NOT in the immediate parent. This can occur if the
    /// index of the parent child is 0.
    /// 
    ///  Level 0                         ___ pk0 ___
    ///                                 /           \
    ///                              pc0             pc1
    ///                             /                   \ 
    ///  Level 1                plk0                     prk0 
    ///                        /   \                    /   \
    ///                     plc0    plc1             prc0    prc1 
    ///                    /           \            /           \
    ///  Level 2         xk0           lk0        rk0           yk0    
    ///                 /   \         /   \      /   \         /   \ 
    ///               xc0   xc1     lc0   lc1  rc0   rc1     yc0   yc1
    /// 
    /// In above example, lc1 and rc0 do not have direct common parent. You need to go 2 levels
    /// up to find middle key pk0.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public (Page Ancestor, int KeyIndex) GetMiddleKey(Page left, Page right)
    {
        // check pages are adjacent
        if (GetRightSibling(left)!.PageId.Value != right.PageId.Value)
        {
            throw new Exception("GetMiddleKey requires pages to be adjacent.");
        }

        var parent = GetChildIndexInParent(right);
        while (parent.ChildIndex == 0)
        {
            parent = GetChildIndexInParent(parent.Parent);
        }
        return (parent.Parent, parent.ChildIndex);
    }

    /// <summary>
    /// Merges 2 pages.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    private void Merge(Page left, Page right)
    {
        /*
        ---------------------------------------------------
                        Parent
                      k1  k2  k3
                    c1  c2  c3  c4    
                       /      \
                      /        \ 
                Left              Right
            k1  k2  k3         k1  k2  k3  k4
          c1  c2  c3  c4     c1  c2  c3  c4  c5

        Steps:
        1. Middle key parent (k2) added to left IF LEFT IS NOT LEAF
        2. add right (k1-k4) to left
        3. add right (c1-c5) to left IF RIGHT + LEFT NOT LEAF
        4. for each child in right (c1-c5), the child pointed to must have parent set to left
        5. add right (buffers) to left
        6. remove Parent (k2)
        7. remove Parent (c3)
        8. remove Right
        9. left becomes new merged node

        NOTES:
        1. Parent (K2) can be in different page to Parent (C2).
        ---------------------------------------------------
        */

        // check pages are adjacent
        if (GetRightSibling(left)!.PageId.Value != right.PageId.Value)
        {
            throw new Exception("Merge requires pages to be adjacent.");
        }

        var isLeftLeaf = left.Header["LEAF"].AsBoolean;
        var leftLocationInParent = GetChildIndexInParent(left);
        var leftKeyCount = left.GetTable(TableIndexEnum.BTREE_KEY).Count();

        // get middle key location
        var middleKeyLocation = GetMiddleKey(left, right);

        // move middle key / buffer to end of left keys
        if (!isLeftLeaf)
        {
            var key = middleKeyLocation.Ancestor.GetRow(TableIndexEnum.BTREE_KEY, middleKeyLocation.KeyIndex);
            var buffer = middleKeyLocation.Ancestor.GetBuffer(TableIndexEnum.BTREE_KEY, middleKeyLocation.KeyIndex);
            left.SetRow(
                TableIndexEnum.BTREE_KEY,
                leftKeyCount,
                key,
                buffer
            );
        }

        // move right keys + buffers to left
        var rightKeyCount = right.GetTable(TableIndexEnum.BTREE_KEY).Count();
        for (int i = 0; i < rightKeyCount; i++)
        {
            left.SetRow(
                TableIndexEnum.BTREE_KEY,
                leftKeyCount + 1 + i,
                right.GetRow(TableIndexEnum.BTREE_KEY, i),
                right.GetBuffer(TableIndexEnum.BTREE_KEY, i)
            );
        }

        // move children keys to left
        if (!isLeftLeaf)
        {
            var leftChildCount = left.GetTable(TableIndexEnum.BTREE_CHILD).Count();
            var rightChildCount = right.GetTable(TableIndexEnum.BTREE_CHILD).Count();
            for (int i = 0; i < rightChildCount; i++)
            {
                var c = right.GetRow(TableIndexEnum.BTREE_CHILD, i);
                var b = right.GetBuffer(TableIndexEnum.BTREE_CHILD, i);
                left.SetRow(
                    TableIndexEnum.BTREE_CHILD,
                    leftChildCount + i,
                    c,
                    b
                );
                // update parent on children
                var ch = BufferManager.Get((int)c["PID"].AsInteger);
                ch.ParentPageId = left.PageId;
            }
        }

        // remove parent key + child
        // if right page occurs at 0th index in parent, need to delete parent c0 and move key0 to middle(left, right)
        // if right page occurs at n>0th index in parent, remove c(n) and k(n-1)
        var location = GetChildIndexInParent(right);
        if (location.ChildIndex > 0)
        {
            location.Parent.DeleteRow(TableIndexEnum.BTREE_CHILD, location.ChildIndex);
            location.Parent.DeleteRow(TableIndexEnum.BTREE_KEY, location.ChildIndex - 1);
        }
        else
        {
            location.Parent.DeleteRow(TableIndexEnum.BTREE_CHILD, location.ChildIndex);
            var keyToMove = location.Parent.GetRow(TableIndexEnum.BTREE_KEY, location.ChildIndex);
            location.Parent.DeleteRow(TableIndexEnum.BTREE_KEY, location.ChildIndex);
            var mk = GetMiddleKey(left, right);
            mk.Ancestor.SetRow(TableIndexEnum.BTREE_KEY, mk.KeyIndex, keyToMove);
        }

        // remove right
        this.BufferManager.Free(right.PageId);
    }

    private bool IsRoot(Page node)
    {
        return node.ParentPageId is not null;
    }

    private Page? GetParent(Page node)
    {
        if (node.ParentPageId is not null)
        {
            return BufferManager.Get(node.ParentPageId.Value);
        }
        else
        {
            return null;
        }
    }

    private void ZeroKeyChangedUpdateParentKey(Page node)
    {
        // updates the parent page if the 0th key on current page changes
        // this can be called recursively

    }

    private void RemoveParentKey(Page node)
    {

    }

    #endregion

}