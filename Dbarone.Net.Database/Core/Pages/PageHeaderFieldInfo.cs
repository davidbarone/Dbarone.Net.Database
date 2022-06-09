namespace Dbarone.Net.Database;
using System.Reflection;

/// <summary>
/// Information about a page header field
/// </summary>
public class PageHeaderFieldInfo
{
    public PropertyInfo Property { get; set; } = default!;
    public int Ordinal { get; set; }
    public int Start { get; set; }
    public int Size { get; set; }
}