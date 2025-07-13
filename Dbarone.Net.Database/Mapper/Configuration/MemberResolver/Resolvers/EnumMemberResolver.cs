namespace Dbarone.Net.Database.Mapper;
using Dbarone.Net.Extensions;

/// <summary>
/// General resolver for enumn types.
/// </summary>
public class EnumMemberResolver : AbstractMemberResolver, IMemberResolver
{
    /// <summary>
    /// Set to true for dictionary and dynamic types, where the member information
    /// must be deferred until mapping time. If set to false, the member information
    /// is obtained at build time.
    /// </summary>
    public override bool DeferBuild => false;

    /// <summary>
    /// Set to true if the current IMemberResolver can resolve members of the specified type.
    /// </summary>
    /// <param name="type">The type to resolve members for.</param>
    /// <returns>Returns true if the current IMemberResolver can resolve members of the specified type.</returns>
    public override bool CanResolveMembersForType(Type type)
    {
        return type.IsEnum;
    }

    /// <summary>
    /// Returns true if types supported by this resolver have members.
    /// </summary>
    public override bool HasMembers => false;

    /// <summary>
    /// Builtin types are not enumerable.
    /// </summary>
    public override bool IsEnumerable => false;
}