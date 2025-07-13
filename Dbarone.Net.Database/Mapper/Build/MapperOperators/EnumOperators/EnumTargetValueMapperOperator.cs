using Dbarone.Net.Database.Mapper;

namespace Dbarone.Net.Database.Mapper;

/// <summary>
/// Operator mapping values to Enum types.
/// </summary>
public class EnumTargetValueMapperOperator : MapperOperator
{
    /// <summary>
    /// Creates a new <see cref="EnumTargetMapperOperator"/> instance.
    /// </summary>
    /// <param name="builder">The <see cref="MapperBuilder"/> instance.</param>
    /// <param name="sourceType">The source <see cref="BuildType"/> instance.</param>
    /// <param name="targetType">The target <see cref="BuildType"/> instance.</param>
    /// <param name="parent">An optional parent <see cref="MapperOperator"/> instance.</param>
    /// <param name="onLog">Optional logging callback.</param>
    public EnumTargetValueMapperOperator(MapperBuilder builder, BuildType sourceType, BuildType targetType, MapperOperator? parent = null, MapperOperatorLogDelegate? onLog = null) : base(builder, sourceType, targetType, parent, onLog)
    {
    }

    protected override IDictionary<string, MapperOperator> GetChildren()
    {
        return new Dictionary<string, MapperOperator>{
            {".", GetChildOperator()}
        };
    }


    /// <summary>
    /// The <see cref="ObjectSourceMapperOperator"/> operator is used to map objects declared as type 'object' at runtime. 
    /// </summary>
    /// <returns>Returns true when the source declared type is 'object'.</returns>
    public override bool CanMap()
    {
        // Get source/target based on Enum underlying type.
        if (TargetType.MemberResolver.GetType() == typeof(EnumMemberResolver))
        {
            var innerSourceTarget = new SourceTarget(SourceType.Type, TargetType.Type.GetEnumUnderlyingType());

            // Target must be enum, and must be able to map the source type -> target underlying type.
            return TargetType.MemberResolver.GetType() == typeof(EnumMemberResolver) &&
            SourceType.Type != typeof(string) &&
            Builder.CanMap(innerSourceTarget);
        }
        else
        {
            return false;
        }
    }

    private MapperOperator GetChildOperator()
    {
        var innerSourceTarget = new SourceTarget(SourceType.Type, TargetType.Type.GetEnumUnderlyingType());
        return Builder.GetMapperOperator(innerSourceTarget, this);
    }

    protected override object? MapInternal(object? source)
    {
        // Get mappede value
        var value = this.Children["."].Map(source);
        var isDefined = Enum.IsDefined(TargetType.Type, value!);

        if (isDefined)
        {
            return (Enum)Enum.ToObject(TargetType.Type, value!);
        }
        else
        {
            throw new MapperRuntimeException($"Value [{source}] cannot be assigned to Enum [{TargetType.Type.Name}].");
        }
    }
}