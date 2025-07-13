using Dbarone.Net.Database.Mapper;

namespace Dbarone.Net.Database.Mapper;

/// <summary>
/// Maps source type to target type if source is assignable to target. Occurs under following scenarios:
/// 1. Source and target are the same type.
/// 2. Source type is derived directly or indirectly from target type.
/// 3. Target type is an interface which source implements.
/// 4. Source is a generic type parameter, and target represents one of the constraints of source.
/// 5. Source represents a value type, and target represents a Nullable version of it.
///
/// For more information, refer to: https://learn.microsoft.com/en-us/dotnet/api/system.type.isassignablefrom?view=net-7.0 
/// </summary>
public class AssignableMapperOperator : MapperOperator
{
    /// <summary>
    /// Creates a new <see cref="AssignableMapperOperator"/> instance. 
    /// </summary>
    /// <param name="builder">The <see cref="MapperBuilder"/> instance.</param>
    /// <param name="sourceType">The source <see cref="BuildType"/> instance.</param>
    /// <param name="targetType">The target <see cref="BuildType"/> instance.</param>
    /// <param name="parent">An optional parent <see cref="MapperOperator"/> instance.</param>
    /// <param name="onLog">Optional logging callback.</param>
    public AssignableMapperOperator(MapperBuilder builder, BuildType sourceType, BuildType targetType, MapperOperator? parent = null, MapperOperatorLogDelegate? onLog = null) : base(builder, sourceType, targetType, parent, onLog) { }

    /// <summary>
    /// The <see cref="AssignableMapperOperator"/> operator is able to map when the source object is assignable to the target type. 
    /// </summary>
    /// <returns>Returns true when the source object is assignable to the target type.</returns>
    public override bool CanMap()
    {
        return TargetType.Type.IsAssignableFrom(SourceType.Type);
    }

    /// <summary>
    /// Mapping implementation for <see cref="AssignableMapperOperator"/> type. 
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <returns>Returns a mapped object.</returns>
    /// <exception cref="MapperBuildException">Returns a <see cref="MapperBuildException"/> in the event of any failure to map the object.</exception>
    protected override object? MapInternal(object? source)
    {
        return source;
    }
}