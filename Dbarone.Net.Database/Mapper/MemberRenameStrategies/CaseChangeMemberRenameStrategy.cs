using Dbarone.Net.Extensions;

namespace Dbarone.Net.Database.Mapper;

/// <summary>
/// Converts member names from specified case type to a common (lowercase). Implementation of <see cref="IMemberRenameStrategy" />.
/// </summary>
public class CaseChangeMemberRenameStrategy : IMemberRenameStrategy
{
    CaseType matchCase { get; set; }
    CaseType newCase { get; set; }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="matchCase">The case to match.</param>
    /// <param name="newCase">The case to change the member to.</param>
    public CaseChangeMemberRenameStrategy(CaseType matchCase, CaseType newCase)
    {
        this.matchCase = matchCase;
        this.newCase = newCase;
    }

    /// <summary>
    /// Renames a member. If the case of the member matches the `matchCase`, then it is converted to `newCase`.
    /// </summary>
    /// <param name="member">The input member name.</param>
    /// <returns>Returns a renamed member name.</returns>
    public string RenameMember(string member)
    {
        if (member.IsCase(matchCase))
        {
            return member.ChangeCase(newCase);
        }
        else
        {
            return member;
        }
    }
}