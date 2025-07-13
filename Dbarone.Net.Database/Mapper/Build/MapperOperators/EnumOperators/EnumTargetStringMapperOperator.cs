using Dbarone.Net.Database.Mapper;

namespace Dbarone.Net.Database.Mapper;

/// <summary>
/// Operator mapping strings to Enum types.
/// </summary>
public class EnumTargetStringMapperOperator : MapperOperator
{
    /// <summary>
    /// Creates a new <see cref="EnumTargetMapperOperator"/> instance.
    /// </summary>
    /// <param name="builder">The <see cref="MapperBuilder"/> instance.</param>
    /// <param name="sourceType">The source <see cref="BuildType"/> instance.</param>
    /// <param name="targetType">The target <see cref="BuildType"/> instance.</param>
    /// <param name="parent">An optional parent <see cref="MapperOperator"/> instance.</param>
    /// <param name="onLog">Optional logging callback.</param>
    public EnumTargetStringMapperOperator(MapperBuilder builder, BuildType sourceType, BuildType targetType, MapperOperator? parent = null, MapperOperatorLogDelegate? onLog = null) : base(builder, sourceType, targetType, parent, onLog)
    {
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
            SourceType.Type == typeof(string);
        }
        else
        {
            return false;
        }
    }

    protected override object? MapInternal(object? source)
    {
        // source guaranteed to be string here.
        return Enum.Parse(TargetType.Type, (string)source!);
    }
}