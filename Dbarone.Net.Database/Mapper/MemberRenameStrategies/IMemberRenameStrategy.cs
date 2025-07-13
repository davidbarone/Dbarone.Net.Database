namespace Dbarone.Net.Database.Mapper;

/// <summary>
/// Interface for classes that can provide member renaming strategies.
/// </summary>
public interface IMemberRenameStrategy
{
    /// <summary>
    /// Renames member names.
    /// </summary>
    /// <param name="member">The member name.</param>
    /// <returns>Returns a modified member name.</returns>
    string RenameMember(string member);
}