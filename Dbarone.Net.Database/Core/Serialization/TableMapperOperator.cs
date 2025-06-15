using Dbarone.Net.Document;
using Dbarone.Net.Mapper;
using Dbarone.Net.Extensions;
using System.Collections;

namespace Dbarone.Net.Database;

/// <summary>
/// Maps IEnumerable types to a <see cref="Table"/> type.
/// </summary>
public class TableMapperOperator : EnumerableMapperOperator
{
    /// <summary>
    /// Creates a new <see cref="EnumerableToTableCellMapperOperator"/> instance.
    /// </summary>
    /// <param name="builder">The <see cref="MapperBuilder"/> instance.</param>
    /// <param name="sourceType">The source <see cref="BuildType"/> instance.</param>
    /// <param name="targetType">The target <see cref="BuildType"/> instance.</param>
    /// <param name="parent">An optional parent <see cref="MapperOperator"/> instance.</param>
    /// <param name="onLog">Optional logging callback.</param>
    public TableMapperOperator(MapperBuilder builder, BuildType sourceType, BuildType targetType, MapperOperator? parent = null, MapperOperatorLogDelegate? onLog = null) : base(builder, sourceType, targetType, parent, onLog)
    {
    }

    /// <summary>
    /// GetChildren implementation for <see cref="TableMapperOperator"/>.
    /// </summary>
    /// <returns>Returns the children operators.</returns>
    /// <exception cref="MapperBuildException"></exception>
    protected override IDictionary<string, MapperOperator> GetChildren()
    {
        // Children
        Dictionary<string, MapperOperator> children = new Dictionary<string, MapperOperator>();
        var fromElementType = SourceType.EnumerableElementType;
        var toElementType = typeof(TableCell);

        if (fromElementType == null)
        {
            throw new MapperBuildException(SourceType.Type, MapperEndPoint.Source, "", "Element type is null.");
        }

        var elementMappingOperator = Builder.GetMapperOperator(new SourceTarget(fromElementType, toElementType), this);
        children["[]"] = elementMappingOperator;
        return children;
    }

    /// <summary>
    /// The <see cref="ObjectSourceMapperOperator"/> operator is used to map objects declared as type 'object' at runtime. 
    /// </summary>
    /// <returns>Returns true when the source declared type is 'object'.</returns>
    public override bool CanMap()
    {
        return SourceType.MemberResolver.IsEnumerable
            && TargetType.Type == typeof(Table);
    }

    /// <summary>
    /// Mapping implementation for <see cref="EnumerableMapperOperator"/> type. 
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <returns>Returns a mapped object.</returns>
    /// <exception cref="MapperBuildException">Returns a <see cref="MapperBuildException"/> in the event of any failure to map the object.</exception>
    protected override object? MapInternal(object? source)
    {
        var arr = source as IEnumerable;
        if (arr == null)
        {
            throw new MapperBuildException(SourceType.Type, MapperEndPoint.Source, this.GetPath(), "Type does not implement IEnumerable.");
        }
        EnumerableBuffer buffer = new EnumerableBuffer(arr, Children["[]"].Map);
        return new Table((TableRow[])buffer.ToArray(typeof(TableRow)));
    }
}