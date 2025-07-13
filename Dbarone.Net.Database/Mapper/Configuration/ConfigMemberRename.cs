namespace Dbarone.Net.Database.Mapper;

/// <summary>
/// Configuration relating to member renaming.
/// </summary>
public class ConfigMemberRename
{
    /// <summary>
    /// The type that the member belongs to.
    /// </summary>
    public Type Type { get; set; } = default!;

    /// <summary>
    /// The (public) member name.
    /// </summary>
    public string MemberName { get; set; } = default!;

    /// <summary>
    /// The (renamed) member name which is used for internal mapping. 
    /// </summary>
    public string InternalMemberName { get; set; } = default!;
}
