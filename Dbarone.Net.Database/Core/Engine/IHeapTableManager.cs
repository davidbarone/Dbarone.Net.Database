/// <summary>
/// Provides an interface for working with heap tables.
/// All table structures in the database can be worked on as
/// a heap table structure. Can be used for data pages as
/// well as system pages with fixed PageData types.
/// </summary>
public interface IHeapTableManager<TRow> {
    
    /// <summary>
    /// Counts the rows in the heap table.
    /// </summary>
    /// <returns></returns>
    int Count();

    /// <summary>
    /// Performs full table scan.
    /// </summary>
    /// <returns></returns>
    IEnumerable<TRow> Scan();

    /// <summary>
    /// Searches for a single row based on a predicate. Returns as soon as first match is found.
    /// </summary>
    /// <typeparam name="TRow">The data type of the row.</typeparam>
    /// <param name="predicate"></param>
    /// <returns></returns>
    DataRowLocation? SearchSingle(Func<TRow, bool> predicate);

    /// <summary>
    /// Searches for multiple rows based on a predicate. Scans entire table.
    /// </summary>
    /// <typeparam name="TRow"></typeparam>
    /// <param name="predicate"></param>
    /// <returns></returns>
    DataRowLocation[] SearchMany(Func<TRow, bool> predicate);

    /// <summary>
    /// Gets a row by its location.
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    TRow GetRow(DataRowLocation location);

    /// <summary>
    /// Adds a row to the heap table.
    /// </summary>
    /// <param name="row">The row to add.</param>
    void AddRow(TRow row);

   /// <summary>
    /// Adds multiple rows to the heap table.
    /// </summary>
    /// <param name="row">The row to add.</param>
    void AddRows(TRow[] row);

    /// <summary>
    /// Updates zero or more rows of the heap table.
    /// </summary>
    /// <param name="row">The new row to update.</param>
    /// <param name="predicate">Rows matching this predicate will be updated.</param>
    void UpdateRows(Func<TRow, TRow> transform, Func<TRow, bool> predicate);

    /// <summary>
    /// Deletes zero or more rows on the heap table.
    /// </summary>
    /// <param name="predicate"></param>
    int DeleteRows(Func<TRow, bool> predicate);
}