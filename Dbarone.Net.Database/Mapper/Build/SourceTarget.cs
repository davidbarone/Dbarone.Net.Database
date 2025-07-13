namespace Dbarone.Net.Database.Mapper;

/// <summary>
/// Represents a source + target tuple.
/// </summary>
public class SourceTarget
{

    /// <summary>
    /// The source type.
    /// </summary>
    public Type Source { get; init; }

    /// <summary>
    /// The target type.
    /// </summary>
    public Type Target { get; init; }

    /// <summary>
    /// Creates a new <see cref="SourceTarget"/>. 
    /// </summary>
    /// <param name="source">The source type.</param>
    /// <param name="target">The target type.</param>
    public SourceTarget(Type source, Type target)
    {
        this.Source = source;
        this.Target = target;
    }

    /// <summary>
    /// Overrides implementation of GetHashCode.
    /// See: https://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-overriding-gethashcode
    /// </summary>
    /// <returns>Returns an integer which can be used to compare 2 instances.</returns>
    public override int GetHashCode()
    {
        int hash = 17;

        // Source + Target cannot be null.
        hash = hash * 23 + Source.GetHashCode();
        hash = hash * 23 + Target.GetHashCode();
        return hash;
    }

    /// <summary>
    /// Overrides implementation of Equals.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object? obj)
    {
        // If parameter is null return false.
        if (obj == null)
        {
            return false;
        }

        // If parameter cannot be cast to SourceTarget return false.
        SourceTarget sd = obj as SourceTarget;
        if ((System.Object?)sd == null)
        {
            return false;
        }

        // Return true if the fields match:
        return (this.Source == sd.Source) && (this.Target == sd.Target);
    }
}