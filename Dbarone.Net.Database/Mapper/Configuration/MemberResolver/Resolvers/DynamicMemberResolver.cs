using Dbarone.Net.Database.Mapper;
using Dbarone.Net.Extensions;
using System.Dynamic;
using System.Linq.Expressions;
using Microsoft.CSharp.RuntimeBinder;
using System.Runtime.CompilerServices;

namespace Dbarone.Net.Database.Mapper;

/// <summary>
/// Member resolver for dynamic types.
/// </summary>
public class DynamicMemberResolver : AbstractMemberResolver, IMemberResolver
{
    public DynamicMemberResolver()
    {
    }

    public override Getter GetGetter(Type type, string memberName, MapperOptions options)
    {
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.csharp.runtimebinder.binder.getmember?view=net-8.0
        // https://stackoverflow.com/questions/5306018/how-to-call-dynamicobject-trygetmember-directly
        // https://stackoverflow.com/questions/12057516/c-sharp-dynamicobject-dynamic-properties
        Getter getter = (obj) =>
        {
            var binder = Binder.GetMember(CSharpBinderFlags.None, memberName, obj.GetType(),
                new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
            var callsite = CallSite<Func<CallSite, object, object>>.Create(binder);
            return callsite.Target(callsite, obj);
        };
        return getter;
    }

    public override string[] GetInstanceMembers(object obj)
    {
        // dynamic types are not forced to expose their properties.
        // Cannot use reflection. We only deal with common types of
        // dynamic object here
        var expObj = obj as IDictionary<string, object>;
        var dynObj = obj as DynamicObject;
        if (expObj != null)
        {
            // must be expando object
            return expObj.Keys.ToArray();
        }
        else if (dynObj != null)
        {
            // dynamic object
            return dynObj.GetDynamicMemberNames().ToArray();
        }
        else
        {
            throw new MapperRuntimeException("Unable to get members for dynamic type.");
        }
    }

    public override Setter GetSetter(Type type, string memberName, MapperOptions options)
    {
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.csharp.runtimebinder.binder.getmember?view=net-8.0
        // https://stackoverflow.com/questions/5306018/how-to-call-dynamicobject-trygetmember-directly
        // https://stackoverflow.com/questions/12057516/c-sharp-dynamicobject-dynamic-properties
        Setter setter = (obj, value) =>
        {
            var binder = Binder.SetMember(CSharpBinderFlags.None, memberName, obj.GetType(),
                new[] {
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
            var callsite = CallSite<Func<CallSite, object, object?, object>>.Create(binder);
            callsite.Target(callsite, obj, value);
        };
        return setter;
    }

    public override bool DeferBuild => true;

    public override bool HasMembers => true;

    public override bool CanResolveMembersForType(Type type)
    {
        return type.IsDynamicType();
    }

    public override bool IsEnumerable => false;
}