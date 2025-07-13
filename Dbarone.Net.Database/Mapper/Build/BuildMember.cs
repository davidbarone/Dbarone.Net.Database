namespace Dbarone.Net.Database.Mapper;
using Dbarone.Net.Extensions;

using System;
using System.Reflection;

/// <summary>
/// Defines the build-time metadata for a member on a type.
/// </summary>
public class BuildMember
{
    /// <summary>
    /// Member name for the mapping rule.
    /// </summary>
    public string MemberName { get; set; } = default!;

    /// <summary>
    /// Member data type of the member.
    /// </summary>
    public Type DataType { get; set; } = default!;

    /// <summary>
    /// The internal member name. Mapping from source to target is done via matching internal names.
    /// </summary>
    public string InternalMemberName { get; set; } = default!;

    /// <summary>
    /// Set to true to ignore this member in the mapping configuration.
    /// </summary>
    internal bool? Ignore { get; set; } = null;

    /// <summary>
    /// Delegate method to get the value from the instance. Returns a <see cref="Getter" /> object.
    /// </summary>
    public Getter? Getter { get; set; } = default!;

    /// <summary>
    /// Delegate method to set the value to the instance. Returns a <see cref="Setter" /> object.
    /// </summary>
    public Setter? Setter { get; set; } = default!;

    /// <summary>
    /// Set to true if a calculation.
    /// </summary>
    public bool IsCalculation => Calculation != null;

    /// <summary>
    /// Used to create a custom calculation. Must be a <see cref="ITypeConverter" /> type.
    /// </summary>
    public ITypeConverter? Calculation { get; set; } = default;
}
