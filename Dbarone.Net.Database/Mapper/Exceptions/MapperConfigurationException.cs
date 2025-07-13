using System.Net;

namespace Dbarone.Net.Database.Mapper;

/// <summary>
/// The MapperConfigurationException class is used for all exceptions throw during the configuration process.
/// </summary>
public class MapperConfigurationException : MapperException
{
    /// <summary>
    /// Creates a new <see cref="MapperConfigurationException"/> instance.
    /// </summary>
    /// <param name="message">The configuration error message.</param>
    public MapperConfigurationException(string message) : base(message)
    {
    }
}