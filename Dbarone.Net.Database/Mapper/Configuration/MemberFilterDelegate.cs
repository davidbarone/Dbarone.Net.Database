namespace Dbarone.Net.Database.Mapper;

/// <summary>
/// Specifies a delegate to filter a member based on the member name.
/// </summary>
/// <param name="memberName">The member name.</param>
/// <returns>Return true to include the member. False to ignore the member.</returns>
public delegate bool MemberFilterDelegate(string memberName);
