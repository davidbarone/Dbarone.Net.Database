using System.Reflection;

namespace Dbarone.Net.Database.Mapper;

/// <summary>
/// Maps types where an implicit cast operator has been defined on either From or To type. Implicit types always succeed and never throw an exception.
/// </summary>
public class ImplicitOperatorMapperOperator : MapperOperator
{
    /// <summary>
    /// Creates a new <see cref="ImplicitOperatorMapperOperator"/> instance.
    /// </summary>
    /// <param name="builder">The <see cref="MapperBuilder"/> instance.</param>
    /// <param name="sourceType">The source <see cref="BuildType"/> instance.</param>
    /// <param name="targetType">The target <see cref="BuildType"/> instance.</param>
    /// <param name="parent">An optional parent <see cref="MapperOperator"/> instance.</param>
    /// <param name="onLog">Optional logging callback.</param>
    public ImplicitOperatorMapperOperator(MapperBuilder builder, BuildType sourceType, BuildType targetType, MapperOperator? parent = null, MapperOperatorLogDelegate? onLog = null) : base(builder, sourceType, targetType, parent, onLog) { }

    private MethodInfo? GetImplicitCast()
    {
        var methods = SourceType.Type.GetMethods(BindingFlags.Public | BindingFlags.Static);
        var method = methods
                        .FirstOrDefault(
                            m => m.GetParameters().Count() > 0 && m.GetParameters()[0].ParameterType == SourceType.Type &&
                            m.ReturnType == TargetType.Type &&
                            m.Name == "op_Implicit"
                        );
        if (method == null)
        {
            method = methods
                            .FirstOrDefault(
                                m => m.GetParameters().Count() > 0 && m.GetParameters()[0].ParameterType == TargetType.Type &&
                                m.ReturnType == SourceType.Type &&
                                m.Name == "op_Implicit"
                            );
        }
        if (method == null)
        {
            methods = TargetType.Type.GetMethods(BindingFlags.Public | BindingFlags.Static);
            method = methods
                            .FirstOrDefault(
                            m => m.GetParameters()[0].ParameterType == SourceType.Type &&
                            m.ReturnType == TargetType.Type &&
                            m.Name == "op_Implicit"
                            );
        }
        if (method == null)
        {
            methods = TargetType.Type.GetMethods(BindingFlags.Public | BindingFlags.Static);
            method = methods
                            .FirstOrDefault(
                            m => m.GetParameters()[0].ParameterType == TargetType.Type &&
                            m.ReturnType == SourceType.Type &&
                            m.Name == "op_Implicit"
                            );

        }
        return method;
    }

    /// <summary>
    /// The <see cref="ImplicitOperatorMapperOperator"/> operator is able to map when an implicit operator exists between the source and target types. 
    /// </summary>
    /// <returns>Returns true when an implicit operator exists between the source and target types.</returns>
    public override bool CanMap()
    {
        // Check if types map directly via assignment
        var implicitOperator = GetImplicitCast();
        return implicitOperator != null;
    }

    /// <summary>
    /// Mapping implementation for <see cref="ImplicitOperatorMapperOperator"/> type. 
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <returns>Returns a mapped object.</returns>
    /// <exception cref="MapperBuildException">Returns a <see cref="MapperBuildException"/> in the event of any failure to map the object.</exception>
    protected override object? MapInternal(object? source)
    {
        var implicitOperator = GetImplicitCast();
        if (implicitOperator == null)
        {
            throw new MapperBuildException(SourceType.Type, MapperEndPoint.Source, "", $"No implicit cast exists to {TargetType.Type}.");
        }
        var result = implicitOperator.Invoke(null, new object?[] { source })!;
        return result;
    }
}