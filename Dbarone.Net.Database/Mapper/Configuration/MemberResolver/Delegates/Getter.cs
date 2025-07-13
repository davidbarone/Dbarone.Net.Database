namespace Dbarone.Net.Database.Mapper;

/// <summary>
/// Defines a basic getter delegate.
/// </summary>
/// <param name="obj">The object providing the value.</param>
/// <returns>Returns a value from the object.</returns>
public delegate object? Getter(object obj);