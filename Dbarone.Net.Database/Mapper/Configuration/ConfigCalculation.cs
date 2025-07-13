namespace Dbarone.Net.Database.Mapper;

/// <summary>
/// Configuration relating to a calculation. A calculation is like a simple mapping, that does not execute recursively.
/// </summary>
public class ConfigCalculation
{
    /// <summary>
    /// The source type that the calculation calculates over.
    /// </summary>
    public Type SourceType { get; set; } = default!;

    /// <summary>
    /// The name of the calculation.
    /// </summary>
    public string MemberName { get; set; } = default!;

    /// <summary>
    /// The type returned by the calculation.
    /// </summary>
    public Type MemberType { get; set; } = default!;

    /// <summary>
    /// The calculation implementation.
    /// </summary>
    public ITypeConverter Calculation { get; set; } = default!;
}
