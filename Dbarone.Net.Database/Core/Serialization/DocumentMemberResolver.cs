using Dbarone.Net.Mapper;
using Dbarone.Net.Document;

namespace Dbarone.Net.Database;

/// <summary>
/// Member resolver for documents.
/// </summary>
public class DocumentMemberResolver : AbstractMemberResolver, IMemberResolver
{
    /// <summary>
    /// Set to true for document types.
    /// </summary>
    public override bool DeferBuild => true;

    public override bool IsEnumerable => true;

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
            var objDict = obj as IDictionary<string, DocumentValue>;
            if (objDict != null)
            {
                if (objDict[memberName].Type == DocumentType.Null)
                {
                    return null;
                }
                else
                {
                    return (object)objDict[memberName];
                }
            }
            else
            {
                throw new Exception("Source object not a valid document type.");
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
            var objDict = target as IDictionary<string, DocumentValue>;
            if (objDict != null)
            {
                if (value is null)
                {
                    objDict[memberName] = DocumentValue.Null;
                }
                else
                {
                    objDict[memberName] = (DocumentValue)value;
                }
            }
            else
            {
                throw new Exception("Target must implement IDictionary<string, DocumentValue>.");
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
        return typeof(DocumentValue);
    }

    /// <summary>
    /// Gets members on an instance. This method is normally implemented only if DeferMemberResolution is set to true.
    /// </summary>
    /// <param name="obj">The object instance.</param>
    /// <returns>A string array of member names.</returns>
    public override string[] GetInstanceMembers(object obj)
    {
        var objDict = obj as IDictionary<string, DocumentValue>;
        if (objDict != null)
        {
            return objDict.Keys.ToArray();
        }
        else
        {
            throw new Exception("Object must implement IDictionary<string, DocumentValue> interface.");
        }
    }

    /// <summary>
    /// Set to true if the current IMemberResolver can resolve members of the specified type.
    /// </summary>
    /// <param name="type">The type to resolve members for.</param>
    /// <returns>Returns true if the current IMemberResolver can resolve members of the specified type.</returns>
    public override bool CanResolveMembersForType(Type type)
    {
        return type.IsAssignableTo(typeof(DocumentValue));
    }

    /// <summary>
    /// Returns true if types supported by this resolver have members.
    /// </summary>
    public override bool HasMembers => true;
}
