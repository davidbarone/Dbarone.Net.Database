namespace Dbarone.Net.Database;

public class Global
{
    /// <summary>
    /// Define the page size as 8K.
    /// </summary>
    public static int PageSize { get { return (int)Math.Pow(2, 13); } }   // 8192
}