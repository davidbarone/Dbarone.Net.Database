namespace Dbarone.Net.Database.Mapper;
using System.Reflection;
using System.Linq.Expressions;

/// <summary>
/// General resolver for interfaces.
/// </summary>
public class InterfaceMemberResolver : ClassMemberResolver, IMemberResolver
{
    /// <summary>
    /// Returns a CreateInstance delegate that can create a new instance of a particular type.
    /// </summary>
    /// <param name="type">The type to create the CreateInstance delegate for.</param>
    /// <param name="args">The arguments to provide to the constructor function to create the new instance.</param>
    /// <returns>Returns a delegate that, when invoked, will create a new instance of an object.</returns>
    public override CreateInstance CreateInstance(Type type, params object?[]? args)
    {
        throw new MapperConfigurationException("CreateInstance delegate does not apply to InterfaceMemberResolver");
    }

    /// <summary>
    /// Set to true if the current IMemberResolver can resolve members of the specified type.
    /// </summary>
    /// <param name="type">The type to resolve members for.</param>
    /// <returns>Returns true if the current IMemberResolver can resolve members of the specified type.</returns>
    public override bool CanResolveMembersForType(Type type)
    {
        return type.IsInterface;
    }
}