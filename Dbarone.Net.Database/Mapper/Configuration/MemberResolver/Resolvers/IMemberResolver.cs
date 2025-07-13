namespace Dbarone.Net.Database.Mapper;
using System.Reflection;

/// <summary>
/// Interface for classes that can perform member resolver services.
/// </summary>
public interface IMemberResolver
{
    /// <summary>
    /// Returns a getter delegate that gets a member value for an object.
    /// </summary>
    /// <param name="type">The type to get the getter for.</param>
    /// <param name="memberName">The member name.</param>
    /// <param name="options">The mapper options provided for the type.</param>
    /// <returns>Returns a getter object which, when invoked, will get a member value from an object.
    /// Returns a null reference if getter does not exist.</returns>
    Getter GetGetter(Type type, string memberName, MapperOptions options);

    /// <summary>
    /// Returns a setter delegate that sets a member value for an object. 
    /// </summary>
    /// <param name="type">The type to get the setter for.</param>
    /// <param name="memberName">The member name</param>
    /// <param name="options">The mapper options provided for the type.</param>
    /// <returns>Returns a setter object which, when invoked, will set a member value for an object.
    /// Returns a null reference if setter does not exist.</returns>
    Setter GetSetter(Type type, string memberName, MapperOptions options);

    /// <summary>
    /// Returns a CreateInstance delegate that can create a new instance of a particular type.
    /// </summary>
    /// <param name="type">The type to create the CreateInstance delegate for.</param>
    /// <param name="args">The arguments to provide to the constructor function to create the new instance.</param>
    /// <returns>Returns a delegate that, when invoked, will create a new instance of an object.</returns>
    CreateInstance CreateInstance(Type type, params object?[]? args);

    /// <summary>
    /// Returns the member names for a type.
    /// </summary>
    /// <param name="type">The type to get the members for.</param>
    /// <param name="options">The options.</param>
    /// <returns></returns>
    string[] GetTypeMembers(Type type, MapperOptions options);

    /// <summary>
    /// Gets members on an instance. This method is normally implemented only if DeferMemberResolution is set to true.
    /// </summary>
    /// <param name="obj">The object instance.</param>
    /// <returns>A string array of member names.</returns>
    string[] GetInstanceMembers(object obj);

    /// <summary>
    /// Gets a member type.
    /// </summary>
    /// <param name="type">The type containing the member.</param>
    /// <param name="memberName">The member name.</param>
    /// <param name="options">The mapper options provided for the type.</param>
    /// <returns>Returns the member type.</returns>
    Type GetMemberType(Type type, string memberName, MapperOptions options);

    /// <summary>
    /// Set to true for dictionary and dynamic types, where the mapper build process
    /// must be deferred until mapping time. If set to false, the mapping process is member information
    /// is obtained at build time.
    /// </summary>
    bool DeferBuild { get; }

    /// <summary>
    /// Returns true if this resolver can return members on a type.
    /// </summary>.
    bool HasMembers { get; }

    /// <summary>
    /// Returns true if the resolver can return an IEnumerable.
    /// </summary>
    bool IsEnumerable { get; }

    /// <summary>
    /// Set to true if the current IMemberResolver can resolve members of the specified type.
    /// </summary>
    /// <param name="type">The type to resolve members for.</param>
    /// <returns>Returns true if the current IMemberResolver can resolve members of the specified type.</returns>
    bool CanResolveMembersForType(Type type);
}