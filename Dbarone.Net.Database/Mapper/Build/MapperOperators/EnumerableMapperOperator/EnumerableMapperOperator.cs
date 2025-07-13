using System.Collections;
using System.Collections.ObjectModel;
using Dbarone.Net.Extensions;
using Dbarone.Net.Database.Mapper;
using System.Collections.Generic;
using System.Linq;

namespace Dbarone.Net.Database.Mapper;

/// <summary>
/// Maps types that implement IEnumerable or have sequences of elements. 
/// </summary>
public class EnumerableMapperOperator : MapperOperator
{
    /// <summary>
    /// Creates a new <see cref="EnumerableMapperOperator"/> instance. 
    /// </summary>
    /// <param name="builder">The <see cref="MapperBuilder"/> instance.</param>
    /// <param name="from">The From <see cref="BuildType"/> instance.</param>
    /// <param name="to">The To <see cref="BuildType"/> instance.</param>
    /// <param name="parent">An optional parent <see cref="MapperOperator"/> instance.</param>
    /// <param name="onLog">Optional logging callback.</param>
    public EnumerableMapperOperator(MapperBuilder builder, BuildType from, BuildType to, MapperOperator? parent = null, MapperOperatorLogDelegate? onLog = null) : base(builder, from, to, parent, onLog)
    {
    }

    /// <summary>
    /// GetChildren implementation for <see cref="EnumerableMapperOperator"/>.
    /// </summary>
    /// <returns>Returns the children operators.</returns>
    /// <exception cref="MapperBuildException"></exception>
    protected override IDictionary<string, MapperOperator> GetChildren()
    {
        // Children
        Dictionary<string, MapperOperator> children = new Dictionary<string, MapperOperator>();
        var fromElementType = SourceType.EnumerableElementType;
        var toElementType = TargetType.EnumerableElementType;

        if (fromElementType == null)
        {
            throw new MapperBuildException(SourceType.Type, MapperEndPoint.Source, "", "Element type is null.");
        }
        if (toElementType == null)
        {
            throw new MapperBuildException(TargetType.Type, MapperEndPoint.Target, "", "Element type is null.");
        }

        var elementMappingOperator = Builder.GetMapperOperator(new SourceTarget(fromElementType, toElementType), this);
        children["[]"] = elementMappingOperator;
        return children;
    }

    /// <summary>
    /// The <see cref="EnumerableMapperOperator"/> type is able to map when both From and To types are enumerable types. 
    /// </summary>
    /// <returns>Returns true when From and To types are enumerable types.</returns>
    public override bool CanMap()
    {
        return
            TargetType.Type.IsEnumerableType() &&
            SourceType.Type.IsEnumerableType() &&
            !TargetType.Type.IsDictionaryType() &&
            !SourceType.Type.IsDictionaryType();
    }

    /// <summary>
    /// Mapping implementation for <see cref="EnumerableMapperOperator"/> type. 
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <returns>Returns a mapped object.</returns>
    /// <exception cref="MapperBuildException">Returns a <see cref="MapperBuildException"/> in the event of any failure to map the object.</exception>
    protected override object? MapInternal(object? source)
    {
        var arr = (source as IEnumerable);
        if (arr == null)
        {
            throw new MapperBuildException(SourceType.Type, MapperEndPoint.Source, this.GetPath(), "Type does not implement IEnumerable.");
        }
        EnumerableBuffer buffer = new EnumerableBuffer(arr, Children["[]"].Map);
        return buffer.To(TargetType.Type);
    }
}