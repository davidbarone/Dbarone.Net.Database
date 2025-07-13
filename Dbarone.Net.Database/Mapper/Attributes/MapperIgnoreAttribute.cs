using System;

namespace Dbarone.Net.Database.Mapper
{
    /// <summary>
    /// Attribute to indicate that a property should not be mapped.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class MapperIgnoreAttribute : Attribute
    {
    }
}