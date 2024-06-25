using Dbarone.Net.Document;

namespace Dbarone.Net.Database;

/// <summary>
/// Options to create a database.
/// </summary>
public class CreateDatabaseOptions
{
    /// <summary>
    /// The page size, expressed as Log2(PageSize). Must be integer between 9 and 16. The default is 13 (page size = 8192). 
    /// </summary>
    public byte PageSize { get; set; } = 13;

    /// <summary>
    /// The text encoding of the database.
    /// </summary>
    public TextEncoding TextEncoding { get; set; } = TextEncoding.UTF8;

    /// <summary>
    /// The percentage of the total page data size that a new row/cell of data must reach to be considered for an overflow page. The default is 25.
    /// </summary>
    public int OverflowThreshold { get; set; } = 25;

    public CreateDatabaseOptions(byte pageSize = 13, TextEncoding textEncoding = TextEncoding.UTF8, int overflowThreshold = 25)
    {
        if (pageSize < 9 || pageSize > 16)
        {
            throw new Exception("PageSize must be between 9 and 16. Default is 13.");
        }

        if (overflowThreshold < 1 || overflowThreshold > 99)
        {
            throw new Exception("OverflowFactor must be between 1 and 99. The default is 25.");
        }

        this.PageSize = pageSize;
        this.TextEncoding = textEncoding;
        this.OverflowThreshold = overflowThreshold;
    }
}