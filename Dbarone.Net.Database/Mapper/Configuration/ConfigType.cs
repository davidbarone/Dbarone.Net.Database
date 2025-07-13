namespace Dbarone.Net.Database.Mapper;

/// <summary>
/// Configuration relating to a type.
/// </summary>
public class ConfigType
{
    /// <summary>
    /// The type being added to configuration
    /// </summary>
    public Type Type { get; set; } = default!;

    /// <summary>
    /// Mapper options relating to the type.
    /// </summary>
    public MapperOptions Options { get; set; } = default!;
}
