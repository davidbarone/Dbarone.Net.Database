namespace Dbarone.Net.Database.Mapper;
using Dbarone.Net.Extensions;

/// <summary>
/// Defines a set of mapping options. These options can be applied to a number of type registrations via the `Register` methods.
/// </summary>
public class MapperOptions
{
    /// <summary>
    /// Set to true to include mapping of fields as well as properties. Default is false.
    /// </summary>
    public bool IncludeFields { get; set; } = false;

    /// <summary>
    /// Set to true to include private fields and properties. Default is false.1
    /// </summary>
    public bool IncludePrivateMembers { get; set; } = false;

    /// <summary>
    /// Optional member renaming strategy. Must implement <see cref="IMemberRenameStrategy" />.
    /// </summary>
    public IMemberRenameStrategy? MemberRenameStrategy { get; set; } = null;

    /// <summary>
    /// Defines implicit assertion of mapping rules prior to any map function call. Defaults to 'None'.
    /// </summary>
    public MapperEndPoint EndPointValidation { get; set; } = MapperEndPoint.None;

    /// <summary>
    /// Provides a member filtering rule.
    /// </summary>
    public MemberFilterDelegate? MemberFilterRule { get; set; } = null;
}