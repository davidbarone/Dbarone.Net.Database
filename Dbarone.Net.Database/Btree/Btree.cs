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
    public void Search()
    {

    }

    public void Insert(TableRow row)
    {

    }

    public void Delete()
    {

    }
}