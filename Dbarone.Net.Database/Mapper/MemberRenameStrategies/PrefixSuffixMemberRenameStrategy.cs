namespace Dbarone.Net.Database.Mapper;

/// <summary>
/// Denotes either prefix or suffix.
/// </summary>
public enum PrefixSuffix
{
    /// <summary>
    /// Prefix (start of string).
    /// </summary>
    Prefix,

    /// <summary>
    /// Suffix (end of string).
    /// </summary>
    Suffix
}

/// <summary>
/// Removes prefix/suffix characters from member names.
/// </summary>
public class PrefixSuffixMemberRenameStrategy : IMemberRenameStrategy
{
    private string stringToRemove { get; set; }
    private PrefixSuffix stringType { get; set; }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="stringType">The <see cref="PrefixSuffix" /> type.</param>
    /// <param name="stringToRemove">The string to remove.</param>
    public PrefixSuffixMemberRenameStrategy(PrefixSuffix stringType, string stringToRemove)
    {
        this.stringType = stringType;
        this.stringToRemove = stringToRemove;
    }

    /// <summary>
    /// Renames a member, removing either a Pre or Post
    /// </summary>
    /// <param name="member"></param>
    /// <returns></returns>
    public string RenameMember(string member)
    {
        if (this.stringType == PrefixSuffix.Prefix && member.StartsWith(stringToRemove, StringComparison.Ordinal))
        {
            return member.Substring(stringToRemove.Length);
        }
        else if (this.stringType == PrefixSuffix.Suffix && member.EndsWith(stringToRemove, StringComparison.Ordinal))
        {
            return member.Substring(0, member.Length - stringToRemove.Length);
        }
        return member;
    }
}