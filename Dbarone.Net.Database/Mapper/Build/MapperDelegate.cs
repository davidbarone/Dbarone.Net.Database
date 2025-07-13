using Dbarone.Net.Database.Mapper;

/// <summary>
/// Delegate which handles mapping of source to target.
/// </summary>
/// <param name="source">The source value.</param>
/// <returns>The mapped / target value.</returns>
public delegate object? MapperDelegate(object? source);