
using System.Reflection;

namespace Dbarone.Net.Database.Mapper;

/// <summary>
/// Maps types based on the presence of a special converter function added to configuration.
/// </summary>
public class ConverterMapperOperator : MapperOperator
{
    /// <summary>
    /// Creates a new <see cref="ConvertibleMapperOperator"/> instance. This operator uses a converter function provided in configuration to map. 
    /// </summary>
    /// <param name="builder">The <see cref="MapperBuilder"/> instance.</param>
    /// <param name="sourceType">The source <see cref="BuildType"/> instance.</param>
    /// <param name="targetType">The target <see cref="BuildType"/> instance.</param>
    /// <param name="parent">An optional parent <see cref="MapperOperator"/> instance.</param>
    /// <param name="onLog">Optional logging callback.</param>
    public ConverterMapperOperator(MapperBuilder builder, BuildType sourceType, BuildType targetType, MapperOperator? parent = null, MapperOperatorLogDelegate? onLog = null) : base(builder, sourceType, targetType, parent, onLog) { }

    /// <summary>
    /// The <see cref="ConverterMapperOperator"/> operator is able to map when a converter function exists between the source and target types. Note that in this case the operator does not recursively map the members. 
    /// </summary>
    /// <returns>Returns true when a converter function exists between the source and target types. Note that in this case the operator does not recursively map the members.</returns>
    public override bool CanMap()
    {
        SourceTarget sourceTarget = new SourceTarget(SourceType.Type, TargetType.Type);
        return Builder.Configuration.Config.Converters.ContainsKey(sourceTarget);
    }

    /// <summary>
    /// Mapping implementation for <see cref="ConverterMapperOperator"/> type. 
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <returns>Returns a mapped object.</returns>
    /// <exception cref="MapperBuildException">Returns a <see cref="MapperBuildException"/> in the event of any failure to map the object.</exception>
    protected override object? MapInternal(object? source)
    {
        SourceTarget sourceTarget = new SourceTarget(SourceType.Type, TargetType.Type);
        var converter = Builder.Configuration.Config.Converters[sourceTarget];

        // Member types differ, but converter exists - convert then assign value to target object.
        var converted = converter.Convert(source);
        return converted;
    }
}