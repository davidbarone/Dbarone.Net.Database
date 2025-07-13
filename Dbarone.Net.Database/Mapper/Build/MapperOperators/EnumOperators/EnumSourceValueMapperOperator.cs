using System.Reflection;
using Dbarone.Net.Database.Mapper;

namespace Dbarone.Net.Database.Mapper;

/// <summary>
/// Operator for when the source type is an enum type. The operator will use the inner type
/// for mapping, or null.
/// </summary>
public class EnumSourceValueMapperOperator : MapperOperator
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
    public EnumSourceValueMapperOperator(MapperBuilder builder, BuildType sourceType, BuildType targetType, MapperOperator? parent = null, MapperOperatorLogDelegate? onLog = null) : base(builder, sourceType, targetType, parent, onLog)
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
        if (SourceType.MemberResolver.GetType() == typeof(EnumMemberResolver))
        {
            var innerSourceTarget = new SourceTarget(SourceType.Type.GetEnumUnderlyingType(), TargetType.Type);

            // Source must be enum, and must be able to map the source underlying type -> target type.
            return SourceType.MemberResolver.GetType() == typeof(EnumMemberResolver) &&
            TargetType.Type != typeof(string) &&
            Builder.CanMap(innerSourceTarget);
        }
        else
        {
            return false;
        }
    }

    private MapperOperator GetChildOperator()
    {
        var innerSourceTarget = new SourceTarget(SourceType.Type.GetEnumUnderlyingType(), TargetType.Type);
        return Builder.GetMapperOperator(innerSourceTarget, this);
    }

    protected override object? MapInternal(object? source)
    {
        if (source is null)
        {
            return null;
        }
        else
        {
            return this.Children["."].Map(source);
        }
    }
}