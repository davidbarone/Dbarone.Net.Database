using Dbarone.Net.Database.Mapper;

namespace Dbarone.Net.Database.Mapper;

/// <summary>
/// Operator for when the source type is a nullable type. The operator will use the inner type
/// for mapping, or null.
/// </summary>
public class NullableSourceMapperOperator : MapperOperator
{
    private MapperOperator? childOperator = null;

    /// <summary>
    /// Creates a new <see cref="ObjectSourceMapperOperator"/> instance.
    /// </summary>
    /// <param name="builder">The <see cref="MapperBuilder"/> instance.</param>
    /// <param name="sourceType">The source <see cref="BuildType"/> instance.</param>
    /// <param name="targetType">The target <see cref="BuildType"/> instance.</param>
    /// <param name="parent">An optional parent <see cref="MapperOperator"/> instance.</param>
    /// <param name="onLog">Optional logging callback.</param>
    public NullableSourceMapperOperator(MapperBuilder builder, BuildType sourceType, BuildType targetType, MapperOperator? parent = null, MapperOperatorLogDelegate? onLog = null) : base(builder, sourceType, targetType, parent, onLog)
    {
    }

    /// <summary>
    /// GetChildren implementation for <see cref="ObjectSourceMapperOperator"/>.
    /// </summary>
    /// <returns>Returns the children operators.</returns>
    /// <exception cref="MapperBuildException"></exception>
    protected override IDictionary<string, MapperOperator> GetChildren()
    {
        return new Dictionary<string, MapperOperator>{
            {"?", GetChildOperator()}
        };
    }

    /// <summary>
    /// The <see cref="ObjectSourceMapperOperator"/> operator is used to map objects declared as type 'object' at runtime. 
    /// </summary>
    /// <returns>Returns true when the source declared type is 'object'.</returns>
    public override bool CanMap()
    {
        var type = SourceType.Type;
        return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
    }

    private MapperOperator GetChildOperator()
    {
        return Builder.GetMapperOperator(
            new SourceTarget(Nullable.GetUnderlyingType(this.SourceType.Type)!,
            this.TargetType.Type
        ), this);
    }

    protected override object? MapInternal(object? source)
    {
        if (source is null)
        {
            return null;
        }
        else
        {
            return this.Children["?"].Map(source);
        }
    }
}