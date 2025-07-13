using System.Net;

namespace Dbarone.Net.Database.Mapper;

/// <summary>
/// The MapperRuntimeException class is used for all exceptions throw during the runtime mapping process.
/// </summary>
public class MapperRuntimeException : MapperException
{
    /// <summary>
    /// Creates a new <see cref="MapperRuntimeException"/> instance.
    /// </summary>
    /// <param name="message">The error message.</param>
    public MapperRuntimeException(string message) : base(message)
    {
    }
}