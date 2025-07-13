namespace Dbarone.Net.Database.Mapper;
using System.Reflection;
using System.Linq.Expressions;

/// <summary>
/// General resolver for structs.
/// </summary>
public class StructMemberResolver : ClassMemberResolver, IMemberResolver
{
    /// <summary>
    /// Returns a CreateInstance delegate that can create a new instance of a particular type.
    /// </summary>
    /// <param name="type">The type to create the CreateInstance delegate for.</param>
    /// <param name="args">The arguments to provide to the constructor function to create the new instance.</param>
    /// <returns>Returns a delegate that, when invoked, will create a new instance of an object.</returns>
    public override CreateInstance CreateInstance(Type type, params object?[]? args)
    {
        List<ParameterExpression> parameters = new List<ParameterExpression>();

        // args array (optional)
        parameters.Add(Expression.Parameter(typeof(object[]), "args"));

        // Create 'new object' expression. The expression must be boxed / cast to object to allow for structs
        var newExp = Expression.Convert(Expression.New(type), typeof(object));

        return Expression.Lambda<CreateInstance>(newExp, parameters).Compile();
    }

    /// <summary>
    /// Set to true if the current IMemberResolver can resolve members of the specified type.
    /// </summary>
    /// <param name="type">The type to resolve members for.</param>
    /// <returns>Returns true if the current IMemberResolver can resolve members of the specified type.</returns>
    public override bool CanResolveMembersForType(Type type)
    {
        return type.IsValueType;
    }
}