
using System.Reflection;

namespace Dbarone.Net.Database.Mapper;

/// <summary>
/// Maps types that support the IConvertible interface. This is typically for built-in types.
/// </summary>
public class ConvertibleMapperOperator : MapperOperator
{
    /// <summary>
    /// Creates a new instance of <see cref="ConvertibleMapperOperator"/>.
    /// </summary>
    /// <param name="builder">The <see cref="MapperBuilder"/> instance.</param>
    /// <param name="sourceType">The From <see cref="BuildType"/> instance.</param>
    /// <param name="targetType">The To <see cref="BuildType"/> instance.</param>
    /// <param name="parent">An optional parent <see cref="MapperOperator"/> instance.</param>
    /// <param name="onLog">Optional logging callback.</param>
    public ConvertibleMapperOperator(MapperBuilder builder, BuildType sourceType, BuildType targetType, MapperOperator? parent = null, MapperOperatorLogDelegate? onLog = null) : base(builder, sourceType, targetType, parent, onLog) { }

    private Type[] ValidToTypes = new Type[] {
            typeof(Byte),
            typeof(char),
            typeof(DateTime),
            typeof(decimal),
            typeof(double),
            typeof(Int16),
            typeof(Int32),
            typeof(Int64),
            typeof(sbyte),
            typeof(Single),
            typeof(string),
            typeof(UInt16),
            typeof(UInt32),
            typeof(UInt64)
        };

    /// <summary>
    /// The <see cref="ConvertibleMapperOperator"/> operator is able to map when the source type implements the IConvertible interface, and can convert to the target type. 
    /// </summary>
    /// <returns>Returns true when the source type implements the IConvertible interface, and can convert to the target type.</returns>
    public override bool CanMap()
    {
        return typeof(IConvertible).IsAssignableFrom(SourceType.Type) && this.ValidToTypes.Contains(TargetType.Type);
    }

    /// <summary>
    /// Mapping implementation for <see cref="ConvertibleMapperOperator"/> type. 
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <returns>Returns a mapped object.</returns>
    /// <exception cref="MapperBuildException">Returns a <see cref="MapperBuildException"/> in the event of any failure to map the object.</exception>
    protected override object? MapInternal(object? source)
    {
        // Member types differ, but converter exists - convert then assign value to target object.
        var iconv = source as IConvertible;
        if (iconv == null)
        {
            throw new MapperRuntimeException("Object does not support IConvertible interface.");
        }
        var converted = iconv.ToType(TargetType.Type, null);
        return converted;
    }
}