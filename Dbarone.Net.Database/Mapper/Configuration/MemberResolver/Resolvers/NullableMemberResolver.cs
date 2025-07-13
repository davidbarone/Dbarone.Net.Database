namespace Dbarone.Net.Database.Mapper;
using Dbarone.Net.Extensions;

/// <summary>
/// General resolver for builtin types.
/// </summary>
public class NullableMemberResolver : AbstractMemberResolver, IMemberResolver
{
    /// <summary>
    /// Set to true for nullable types.
    /// </summary>
    public override bool DeferBuild => false;

    /// <summary>
    /// Set to true if the current IMemberResolver can resolve members of the specified type.
    /// </summary>
    /// <param name="type">The type to resolve members for.</param>
    /// <returns>Returns true if the current IMemberResolver can resolve members of the specified type.</returns>
    public override bool CanResolveMembersForType(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    /// <summary>
    /// Nullable types not considered to have members.
    /// </summary>
    public override bool HasMembers => false;

    /// <summary>
    /// Nullable types are not enumerable.
    /// </summary>
    public override bool IsEnumerable => false;
}