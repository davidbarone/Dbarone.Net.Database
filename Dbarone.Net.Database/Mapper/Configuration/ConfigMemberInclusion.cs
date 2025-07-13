namespace Dbarone.Net.Database.Mapper;

/// <summary>
/// Configuration of member inclusion / exclusion for mapping.
/// </summary>
public class ConfigMemberInclusion
{
    /// <summary>
    /// The type that the member belongs to.
    /// </summary>
    public Type Type { get; set; } = default!;

    /// <summary>
    /// The (public) member name.
    /// </summary>
    public string Member { get; set; } = default!;

    /// <summary>
    /// The include / exclude behaviour for mapping.
    /// </summary>
    public IncludeExclude IncludeExclude { get; set; }
}
