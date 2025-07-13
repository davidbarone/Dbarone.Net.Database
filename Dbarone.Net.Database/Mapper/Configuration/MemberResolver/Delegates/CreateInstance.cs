namespace Dbarone.Net.Database.Mapper;

/// <summary>
/// Delegate to create a new instance of an object.
/// </summary>
/// <param name="args">Optional args to pass into the constructor.</param>
/// <returns>Returns an instance of an object.</returns>
public delegate object CreateInstance(params object?[]? args);