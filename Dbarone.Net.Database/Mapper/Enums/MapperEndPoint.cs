namespace Dbarone.Net.Database.Mapper;

/// <summary>
/// Defines the mapper end point type.
/// </summary>
[Flags]
public enum MapperEndPoint
{
    /// <summary>
    /// No end point type specified.
    /// </summary>
    None = 0,

    /// <summary>
    /// Source mapper endpoint.
    /// </summary>
    Source = 1,

    /// <summary>
    /// Target mapper endpoint.
    /// </summary>
    Target = 2,
}