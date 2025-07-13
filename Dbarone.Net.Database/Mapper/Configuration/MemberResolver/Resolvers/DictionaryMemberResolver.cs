namespace Dbarone.Net.Database.Mapper;
using System.Reflection;
using System.Linq.Expressions;
using Dbarone.Net.Extensions;

/// <summary>
/// Member resolver for dictionaries.
/// </summary>
public class DictionaryMemberResolver : AbstractMemberResolver, IMemberResolver
{
    /// <summary>
    /// Returns a getter delegate that gets a member value for an object.
    /// </summary>
    /// <param name="type">The type to get the getter for.</param>
    /// <param name="memberName">The member name.</param>
    /// <param name="options">The mapper options provided for the type.</param>
    /// <returns>Returns a getter object which, when invoked, will get a member value from an object.
    /// Returns a null reference if getter does not exist.</returns>
    public override Getter GetGetter(Type type, string memberName, MapperOptions options)
    {
        Getter func = (object obj) =>
        {
            var objDict = obj as IDictionary<string, object>;
            if (objDict != null)
            {
                return (object)objDict[memberName];
            }
            else
            {
                throw new Exception("Source object not a valid dictionary type.");
            }
        };
        return func;
    }

    /// <summary>
    /// Returns a setter delegate that sets a member value for an object. 
    /// </summary>
    /// <param name="type">The type to get the setter for.</param>
    /// <param name="memberName">The member name</param>
    /// <param name="options">The mapper options provided for the type.</param>
    /// <returns>Returns a setter object which, when invoked, will set a member value for an object.
    /// Returns a null reference if setter does not exist.</returns>
    public override Setter GetSetter(Type type, string memberName, MapperOptions options)
    {
        Setter action = delegate (object target, object? value)
        {
            var objDict = target as IDictionary<string, object>;
            if (objDict != null)
            {
                objDict[memberName] = value;
            }
            else
            {
                throw new Exception("Target must implement IDictionary<string, object>.");
            }
        };
        return action;
    }

    /// <summary>
    /// Gets a member type.
    /// </summary>
    /// <param name="type">The type containing the member.</param>
    /// <param name="memberName">The member name.</param>
    /// <param name="options">The mapper options provided for the type.</param>
    /// <returns>Returns the member type.</returns>
    public override Type GetMemberType(Type type, string memberName, MapperOptions options)
    {
        return typeof(object);
    }

    /// <summary>
    /// Gets members on an instance. This method is normally implemented only if DeferMemberResolution is set to true.
    /// </summary>
    /// <param name="obj">The object instance.</param>
    /// <returns>A string array of member names.</returns>
    public override string[] GetInstanceMembers(object obj)
    {
        var objDict = obj as IDictionary<string, object>;
        if (objDict != null)
        {
            return objDict.Keys.ToArray();
        }
        else
        {
            throw new Exception("Object must implement IDictionary<string, object> interface.");
        }
    }

    /// <summary>
    /// Returns the member names for a type.
    /// </summary>
    /// <param name="type">The type to get the members for.</param>
    /// <param name="options">The options.</param>
    /// <returns></returns>
    public override string[] GetTypeMembers(Type type, MapperOptions options)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Set to true if the current IMemberResolver can resolve members of the specified type.
    /// </summary>
    /// <param name="type">The type to resolve members for.</param>
    /// <returns>Returns true if the current IMemberResolver can resolve members of the specified type.</returns>
    public override bool CanResolveMembersForType(Type type)
    {
        return type.IsDictionaryType();
    }

    /// <summary>
    /// Returns true if types supported by this resolver have members.
    /// </summary>
    public override bool HasMembers => true;

    /// <summary>
    /// Set to true for dictionary and dynamic types, where the member information
    /// must be deferred until mapping time. If set to false, the member information
    /// is obtained at build time.
    /// </summary>
    public override bool DeferBuild => true;

    /// <summary>
    /// DictionaryMemberResolver does not support Enumerable types.
    /// </summary>
    public override bool IsEnumerable => false;
}