namespace Dbarone.Net.Database;

[System.AttributeUsage(System.AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
public class PageHeaderAttribute : System.Attribute
{
    /// <summary>
    /// Ordinal position of field.
    /// </summary>
    public int Ordinal { get; set; }
    
    /// <summary>
    /// Maximum length for variable length fields. Headers with variable length data types are stored as fixed length (MaxLength).
    /// </summary>
    public int MaxLength { get; set; }
    
    public PageHeaderAttribute(int ordinal, int maxLength = 0) {
        this.Ordinal = ordinal;
        this.MaxLength = maxLength;
    }
}