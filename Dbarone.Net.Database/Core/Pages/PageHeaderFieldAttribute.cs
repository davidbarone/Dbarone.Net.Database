namespace Dbarone.Net.Database;

[System.AttributeUsage(System.AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
public class PageHeaderFieldAttribute : System.Attribute
{
    /// <summary>
    /// Ordinal position of field.
    /// </summary>
    public int Ordinal { get; set; }
    
    /// <summary>
    /// Maximum length for variable length fields.
    /// </summary>
    public int MaxLength { get; set; }
    
    public PageHeaderFieldAttribute(int ordinal, int maxLength = 0) {
        this.Ordinal = ordinal;
        this.MaxLength = maxLength;
    }
}