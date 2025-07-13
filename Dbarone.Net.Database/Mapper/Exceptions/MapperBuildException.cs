using System.Net;

namespace Dbarone.Net.Database.Mapper;

/// <summary>
/// The MapperBuild exception class is used for all exceptions throw during the build process of the mapper.
/// </summary>
public class MapperBuildException : MapperException
{
    /// <summary>
    /// The list of notifications occurring during the build process.
    /// </summary>
    public List<MapperBuildError> Errors { get; set; } = new List<MapperBuildError>();

    /// <summary>
    /// Creates a new <see cref="MapperBuildException"/>.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="errors">The build errors.</param>
    public MapperBuildException(string message, List<MapperBuildError> errors) : base(message)
    {
        this.Errors = errors;
    }

    /// <summary>
    /// Creates a new <see cref="MapperBuildException"/>.
    /// </summary>
    /// <param name="errors">The build errors.</param>
    public MapperBuildException(List<MapperBuildError> errors) : base("An error has occurred during the build phase. Refer to the inner Errors property for details.")
    {
        this.Errors = errors;
    }

    /// <summary>
    /// Creates a new <see cref="MapperBuildException"/>.
    /// </summary>
    /// <param name="type">The type generating the error(s).</param>
    /// <param name="endPoint">Signifies whether the type is a source or target type.</param>
    /// <param name="memberName">Optional member name.</param>
    /// <param name="message">The error message.</param>
    public MapperBuildException(Type type, MapperEndPoint endPoint, string memberName, string message) : base("An error has occurred during the build phase. Refer to the inner Errors property for details.")
    {
        this.Errors = new List<MapperBuildError>{
            new MapperBuildError(type, endPoint, memberName, message)
        };
    }
}