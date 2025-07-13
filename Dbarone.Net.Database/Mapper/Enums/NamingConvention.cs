namespace Dbarone.Net.Database.Mapper;

/// <summary>
/// Defines the default member naming convention for the type.
/// </summary>
public enum NamingConvention
{


    /// <summary>
    /// No defined naming convention used.
    /// </summary>
    None,

    /// <summary>
    /// Members are named in CamelCase, for example 'memberName'.
    /// </summary>
    CamelCaseNamingConvention,

    /// <summary>
    /// Members are named in PascalCase, for example: 'MemberName'.
    /// </summary>
    PascalCaseNamingConvention,

    /// <summary>
    /// Members are named in SnakeCase, for example 'member_name'.
    /// </summary>
    SnakeCasingConvention

}