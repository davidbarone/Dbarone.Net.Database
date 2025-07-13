using System.Diagnostics;
using System.Reflection;
using Dbarone.Net.Extensions;

namespace Dbarone.Net.Database.Mapper;

/// <summary>
/// Base class for all mapper operator types.
/// </summary>
public abstract class MapperOperator
{
    /// <summary>
    /// Reference to the current MapperBuilder instance.
    /// </summary>
    protected MapperBuilder Builder { get; set; }

    /// <summary>
    /// The source <see cref="BuildType"/> object.
    /// </summary>
    protected BuildType SourceType { get; set; }

    /// <summary>
    /// The target <see cref="BuildType"/> object.
    /// </summary>
    protected BuildType TargetType { get; set; }

    /// <summary>
    /// Optional parent <see cref="MapperOperator"/> reference.
    /// </summary>
    protected MapperOperator? Parent { get; set; }

    /// <summary>
    /// Total duration of the mapper operator.
    /// </summary>
    protected Stopwatch Stopwatch { get; set; }

    /// <summary>
    /// Number of iterations of the current mapper operator. 
    /// </summary>
    protected int Count { get; set; }

    /// <summary>
    /// The number of mapper operations per second.
    /// </summary>
    public int Rate => Stopwatch.ElapsedMilliseconds == 0 ? 0 : (int)(Count * 1000 / Stopwatch.ElapsedMilliseconds);

    /// <summary>
    /// Logging callback function.
    /// </summary>
    protected MapperOperatorLogDelegate? OnLog { get; set; }

    /// <summary>
    /// Create a new <see cref="MapperOperator"/> instance.
    /// </summary>
    /// <param name="builder">A <see cref="MapperBuilder"/> instance.</param>
    /// <param name="sourceType">The source <see cref="BuildType"/>.</param>
    /// <param name="targetType">The target <see cref="BuildType"/>.</param>
    /// <param name="parent">Optional parent <see cref="MapperOperator"/> instance.</param>
    /// <param name="onLog">Optional logging callback.</param>
    public MapperOperator(MapperBuilder builder, BuildType sourceType, BuildType targetType, MapperOperator? parent = null, MapperOperatorLogDelegate? onLog = null)
    {
        this.Parent = parent;
        this.Builder = builder;
        this.SourceType = sourceType;
        this.TargetType = targetType;
        this.Stopwatch = new Stopwatch();
        this.OnLog = onLog;
    }

    /// <summary>
    /// Returns true if the current class can map the source / target types.
    /// </summary>
    /// <returns>Returns true if the current mapper can map the source / target types.</returns>
    public abstract bool CanMap();

    /// <summary>
    /// Maps a source object, returning a mapped object.
    /// </summary>
    /// <param name="source">The source object</param>
    /// <returns>A mapped object.</returns>
    public object? Map(object? source)
    {
        this.Stopwatch.Start();
        var result = MapInternal(source);
        this.Stopwatch.Stop();
        this.Count++;

        // log
        if (this.OnLog != null)
        {
            this.OnLog(this, MapperOperatorLogType.Runtime);
        }

        return result;
    }

    /// <summary>
    /// Sub type specific implementation of mapping operator.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <returns>A mapped object.</returns>
    protected abstract object? MapInternal(object? source);

    /// <summary>
    /// Function to get the children of the current operation. Must be implemented in subclasses. This function is called by the Children property.
    /// </summary>
    /// <returns>Returns the children operations. If no children for the operation, call the base implementation.</returns>
    protected virtual IDictionary<string, MapperOperator> GetChildren()
    {
        return new Dictionary<string, MapperOperator>();
    }

    private IDictionary<string, MapperOperator>? _children = default!;

    /// <summary>
    /// Returns the children of the current operation.
    /// </summary>
    public IDictionary<string, MapperOperator> Children
    {
        get
        {
            if (_children == null)
            {
                _children = GetChildren();
            }
            return _children;
        }
    }

    /// <summary>
    /// Resets / clears the child operators for the current node only.
    /// </summary>
    protected void ResetChildren()
    {
        _children = null;
    }

    /// <summary>
    /// Static method to determine whether a source-target mapping exists.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="sourceType"></param>
    /// <param name="targetType"></param>
    /// <param name="parent"></param>
    /// <param name="onLog"></param>
    /// <returns></returns>
    /// <exception cref="MapperBuildException"></exception>
    public static bool CanMap(MapperBuilder builder, BuildType sourceType, BuildType targetType, MapperOperator? parent = null, MapperOperatorLogDelegate? onLog = null)
    {
        // Get all the mapper operator types
        // Default operators come last.
        var operatorTypes = builder.Configuration.Config.Operators;

        List<MapperOperator> mapperOperators = new List<MapperOperator>();
        foreach (var type in operatorTypes)
        {
            var mapperOperator = (MapperOperator?)Activator.CreateInstance(type, builder, sourceType, targetType, parent, onLog);
            if (mapperOperator == null)
            {
                throw new MapperBuildException(sourceType.Type, MapperEndPoint.Source, "", $"Unable to create mapper operator for mapper type: {type.Name}.");
            }
            mapperOperators.Add(mapperOperator);
        }
        mapperOperators = mapperOperators.ToList();

        // Find the first mapper operator that can map the types
        foreach (var mapperOperator in mapperOperators)
        {
            if (mapperOperator.CanMap())
            {
                // raise log event
                if (onLog != null)
                {
                    onLog(mapperOperator, MapperOperatorLogType.Build);
                }
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Factory method to create a new MapperOperator instance based on from / to types.
    /// </summary>
    /// <param name="builder">The current <see cref="MapperBuilder"/> instance.</param>
    /// <param name="sourceType">The source type.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parent">An optional parent operator.</param>
    /// <param name="onLog">An optional <see cref="MapperOperatorLogDelegate"/> callback function. This callback function called by the logging subsystem.</param>
    /// <returns>A <see cref="MapperOperator"/> instance that can map from / to types.</returns>
    /// <exception cref="MapperBuildException">Throws an exception if no suitable mapper found.</exception>
    public static MapperOperator Create(MapperBuilder builder, BuildType sourceType, BuildType targetType, MapperOperator? parent = null, MapperOperatorLogDelegate? onLog = null)
    {
        // Get all the mapper operator types
        // Default operators come last.
        var operatorTypes = builder.Configuration.Config.Operators;

        List<MapperOperator> mapperOperators = new List<MapperOperator>();
        foreach (var type in operatorTypes)
        {
            var mapperOperator = (MapperOperator?)Activator.CreateInstance(type, builder, sourceType, targetType, parent, onLog);
            if (mapperOperator == null)
            {
                throw new MapperBuildException(sourceType.Type, MapperEndPoint.Source, "", $"Unable to create mapper operator for mapper type: {type.Name}.");
            }
            mapperOperators.Add(mapperOperator);
        }
        mapperOperators = mapperOperators.ToList();

        // Find the first mapper operator that can map the types
        foreach (var mapperOperator in mapperOperators)
        {
            if (mapperOperator.CanMap())
            {
                // raise log event
                if (onLog != null)
                {
                    onLog(mapperOperator, MapperOperatorLogType.Build);
                }
                return mapperOperator;
            }
        }

        // Shouldn't get here.
        throw new MapperBuildException(sourceType.Type, MapperEndPoint.Source, "", $"Cannot map to ${targetType.Type.Name} type.");
    }

    /// <summary>
    /// Gets the path of the current <see cref="MapperOperator"/>.
    /// </summary>
    /// <returns>Returns a string value representing the path of the current operator.</returns>
    /// <exception cref="Exception">Throws an exception if an invalid child.</exception>
    public string GetPath()
    {
        if (this.Parent == null)
        {
            return "";
        }
        else
        {
            foreach (var key in this.Parent.Children.Keys)
            {
                var item = this.Parent.Children[key];
                if (Object.ReferenceEquals(item, this))
                {
                    return $"{this.Parent.GetPath()}.{key}";
                }
            }
            throw new Exception("shouldn't get here");
        }
    }

    /// <summary>
    /// Pretty-prints the current mapper operator
    /// </summary>
    /// <param name="indent">The current indentation level.</param>
    /// <param name="key">The key</param>
    /// <param name="isLastChild">Optional - set to true if the last child. Used to change the indentation characters.</param>
    /// <returns>String representation of the mapper operator.</returns>
    public string PrettyPrint(string indent = "", string key = "", bool isLastChild = true)
    {
        if (key != "")
        {
            key = key + ": ";
        }
        var output = $"{indent}+- {key}{this.GetType().Name} ({this.SourceType.Type.Name}->{this.TargetType.Type.Name}){Environment.NewLine}";
        indent += isLastChild ? "   " : "|  ";

        // print children too
        List<string> childKeys = this.Children.Keys.ToList();

        for (int i = 0; i < childKeys.Count(); i++)
        {
            var childKey = childKeys[i];
            var child = this.Children[childKey];
            // the child operator can be null, for example for operators built at runtime.
            if (child is null)
            {
                // The child is not yet defined
                output += $"{indent}+- {childKey}: <undefined>{Environment.NewLine}";
            }
            else
            {
                output += child.PrettyPrint(indent, childKey, i == childKeys.Count() - 1);
            }
        }
        return output;
    }
}