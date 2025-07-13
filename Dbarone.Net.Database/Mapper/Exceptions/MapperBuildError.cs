using System.Net;

namespace Dbarone.Net.Database.Mapper;

/// <summary>
/// A single build-time error raised during the mapper build process.
/// </summary>
public class MapperBuildError
{

    /// <summary>
    /// The type relating to the mapper build error.  
    /// </summary>
    public Type Type { get; set; } = default!;

    /// <summary>
    /// The endpoint relating to the mapper build error.
    /// </summary>
    public MapperEndPoint EndPoint { get; set; } = default;

    /// <summary>
    /// The optional member relating to the mapper build error.
    /// </summary>
    public string? MemberName { get; set; } = default!;

    /// <summary>
    /// The message relating to the mapper build error. 
    /// </summary>
    public string Message { get; set; } = default!;

    /// <summary>
    /// Creates a new MapperBuildError instance.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="endPoint">The end point type.</param>
    /// <param name="memberName">The optional member name</param>
    /// <param name="message">The error message.</param>
    public MapperBuildError(Type type, MapperEndPoint endPoint, string? memberName, string message)
    {
        this.Type = type;
        this.EndPoint = endPoint;
        this.MemberName = memberName;
        this.Message = message;
    }
}