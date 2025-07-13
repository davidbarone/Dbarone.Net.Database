namespace Dbarone.Net.Database.Mapper;

/// <summary>
/// Interface for classes that can convert values / types.
/// </summary>
public interface ITypeConverter
{
    /// <summary>
    /// Converts an object.
    /// </summary>
    /// <param name="obj">The object to be converted.</param>
    /// <returns>A converted object.</returns>
    public object? Convert(object? obj);
}
