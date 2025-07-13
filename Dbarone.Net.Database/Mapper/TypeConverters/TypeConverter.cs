namespace Dbarone.Net.Database.Mapper;

/// <summary>
/// Converts an object using a generic lambda function or Func. 
/// </summary>
/// <typeparam name="T">The source type.</typeparam>
/// <typeparam name="U">The target type.</typeparam>
public class TypeConverter<T, U> : ITypeConverter
{
    private Func<T?, U?> converter;

    /// <summary>
    /// Creates a TypeConverter instance using a m
    /// </summary>
    /// <param name="converter"></param>
    public TypeConverter(Func<T?, U?> converter)
    {
        this.converter = converter;
    }

    /// <summary>
    /// Implementation of interface Convert method.
    /// </summary>
    /// <param name="obj">The object to be converted.</param>
    /// <returns>A converted object.</returns>
    public object? Convert(object? obj)
    {
        return (object?)converter.Invoke((T?)obj);
    }
}