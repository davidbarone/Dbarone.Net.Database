using System.Collections;
using System.Dynamic;
using Dbarone.Net.Extensions;
using Dbarone.Net.Database.Mapper;

namespace Dbarone.Net.Extensions;

/// <summary>
/// Buffer to convert an IEnumerable to other common IEnumerable types (generic and non-generic).
/// </summary>
public class EnumerableBuffer
{
    private int Count { get; init; }
    private object?[] Buffer { get; init; }

    /// <summary>
    /// Generic convert-to method.
    /// </summary>
    /// <typeparam name="T">The type to convert the IEnumerable to. Must be one of the common enumerable or collection types.</typeparam>
    /// <returns>Returns the collection, cast to the specific type.</returns>
    public T To<T>()
    {
        return (T)To(typeof(T));
    }

    /// <summary>
    /// Converts an <see cref="EnumerableBuffer"/> instance to another enumerable or collection type.
    /// </summary>
    /// <param name="type">The type to convert to.</param>
    /// <returns>Returns the EnumerableBuffer, converted to the specified type.</returns>
    /// <exception cref="MapperRuntimeException">Throws an exception if the specified type is not one of the common enumerable or collection types.</exception>
    public IEnumerable To(Type type)
    {
        var elementType = type.GetEnumerableElementType();
        if (elementType == null)
        {
            throw new MapperRuntimeException($"Type: {type.Name} does not have an element type.");
        }

        if (type == typeof(ArrayList))
        {
            return ToArrayList();
        }
        else if (type == typeof(Queue))
        {
            return ToQueue();
        }
        else if (type == typeof(Stack))
        {
            return ToStack();
        }
        else if (type.IsArray)
        {
            return ToArray(elementType);
        }
        else if (type.IsAssignableToGenericType(typeof(IList<>)))
        {
            return ToGenericList(elementType);
        }
        else if (type.IsAssignableToGenericType(typeof(IEnumerable<>)))
        {
            return ToGenericEnumerable(elementType);
        }
        throw new MapperRuntimeException($"Cannot convert EnumerableBuffer to type: {type.Name}.");
    }

    /// <summary>
    /// Converts the <see cref="EnumerableBuffer"/> instance to an <see cref="ArrayList"/>.
    /// </summary>
    /// <returns>Returns the <see cref="EnumerableBuffer"/> instance as an <see cref="ArrayList"/>.</returns>
    public ArrayList ToArrayList()
    {
        return new ArrayList(this.Buffer);
    }

    /// <summary>
    /// Converts the <see cref="EnumerableBuffer"/> instance to a <see cref="Queue"/>.
    /// </summary>
    /// <returns>Returns the <see cref="EnumerableBuffer"/> instance as a <see cref="Queue"/>.</returns>
    public Queue ToQueue()
    {
        return new Queue(this.Buffer);
    }

    /// <summary>
    /// Converts the <see cref="EnumerableBuffer"/> instance to a <see cref="Stack"/>.
    /// </summary>
    /// <returns>Returns the <see cref="EnumerableBuffer"/> instance as a <see cref="Stack"/>.</returns>
    public Stack ToStack()
    {
        return new Stack(this.Buffer);
    }

    /// <summary>
    /// Converts the <see cref="EnumerableBuffer"/> instance to a generic list.
    /// </summary>
    /// <typeparam name="T">The type of the generic list elements.</typeparam>
    /// <returns>Returns the <see cref="EnumerableBuffer"/> instance as a generic list.</returns>
    public IList<T> ToGenericList<T>()
    {
        return this.Buffer.Cast<T>().ToList();
    }

    /// <summary>
    /// Converts the <see cref="EnumerableBuffer"/> instance to a generic list.
    /// </summary>
    /// <param name="elementType">The type of the generic list elements.</param>
    /// <returns>Returns the <see cref="EnumerableBuffer"/> instance as a generic list.</returns>
    public IList ToGenericList(Type elementType)
    {
        // Get the cast method
        var castMethod = typeof(IEnumerable).GetExtensionMethods().First(m => m.Name == "Cast");
        // Get the cast method for the element type parameter, and invoke;
        var results = castMethod.MakeGenericMethod(elementType).Invoke(null, new object[] { this.Buffer });
        if (results == null)
        {
            throw new MapperRuntimeException("Null return value for ToGenericList().");
        }
        var toListMethod = typeof(IEnumerable<>).GetExtensionMethods().First(m => m.Name == "ToList");
        var returnValue = toListMethod.MakeGenericMethod(elementType).Invoke(null, new object[] { results }) as IList;
        if (returnValue == null)
        {
            throw new MapperRuntimeException("Null return value for ToGenericList().");
        }
        return returnValue;
    }

    /// <summary>
    /// Converts the <see cref="EnumerableBuffer"/> instance to a generic IEnumerable.
    /// </summary>
    /// <typeparam name="T">The type of the generic IEnumerable elements.</typeparam>
    /// <returns>Returns the <see cref="EnumerableBuffer"/> instance as a generic IEnumerable.</returns>
    public IEnumerable<T> ToGenericEnumerable<T>()
    {
        return this.Buffer.Cast<T>();
    }

    /// <summary>
    /// Converts the <see cref="EnumerableBuffer"/> instance to a generic IEnumerable.
    /// </summary>
    /// <param name="elementType">The type of the generic IEnumerable elements.</param>
    /// <returns>Returns the <see cref="EnumerableBuffer"/> instance as a generic IEnumerable.</returns>
    public IEnumerable ToGenericEnumerable(Type elementType)
    {
        var castMethod = typeof(IEnumerable).GetExtensionMethods().First(m => m.Name == "Cast");
        // Get the cast method for the element type parameter, and invoke;
        var results = castMethod.MakeGenericMethod(elementType).Invoke(null, new object[] { this.Buffer }) as IEnumerable;
        if (results == null)
        {
            throw new MapperRuntimeException("Null return value for ToGenericEnumerable().");
        }
        return results;
    }

    /// <summary>
    /// Converts the <see cref="EnumerableBuffer"/> instance to a generic <see cref="Array"/>.
    /// </summary>
    /// <typeparam name="T">The type of the generic Array elements.</typeparam>
    /// <returns>Returns the <see cref="EnumerableBuffer"/> instance as a generic <see cref="Array"/>.</returns>
    public T[] ToArray<T>()
    {
        return this.Buffer.Cast<T>().ToArray<T>();
    }

    /// <summary>
    /// Converts the <see cref="EnumerableBuffer"/> instance to a generic <see cref="Array"/>.
    /// </summary>
    /// <param name="elementType">The type of the Array elements.</param>
    /// <returns>Returns the <see cref="EnumerableBuffer"/> instance as a generic <see cref="Array"/>.</returns>
    public Array ToArray(Type elementType)
    {
        var list = this.ToGenericList(elementType);

        var castMethod = typeof(IEnumerable<>).GetExtensionMethods().First(m => m.Name == "ToArray");
        // Get the cast method for the element type parameter, and invoke;
        var results = castMethod.MakeGenericMethod(elementType).Invoke(null, new object[] { list }) as Array;
        if (results == null)
        {
            throw new MapperRuntimeException("Null return value for ToArray().");
        }
        return results;
    }

    /// <summary>
    /// Constructor to create a new <see cref="EnumerableBuffer"/> instance.
    /// </summary>
    /// <param name="source">The source IEnumerable object.</param>
    /// <param name="mapper">An optional mapper function. If provided, this function will be applied on the source.</param>
    public EnumerableBuffer(IEnumerable source, MapperDelegate? mapper = null)
    {
        object?[] buffer = default!;
        int count = 0;
        ICollection? collection = source as ICollection;
        bool isGeneric = source.GetType().IsGenericType;

        if (collection != null && mapper == null)
        {
            // We can use Count property
            count = collection.Count;
            if (count > 0)
            {
                buffer = new object[count];
                collection.CopyTo(buffer, 0);
            }
        }
        else
        {
            // We'll have to loop through items.
            foreach (var element in source)
            {
                if (buffer == null)
                {
                    buffer = new object[8];   // initialise, size=8
                }
                else if (buffer.Length == count)
                {
                    object[] newBuffer = new object[checked(count * 2)];
                    Array.Copy(buffer, 0, newBuffer, 0, count);
                    buffer = newBuffer;
                }
                var item = element;
                if (mapper != null)
                {
                    item = mapper(item);
                }

                buffer[count] = item;
                count++;
            }
        }

        if (count == 0)
        {
            this.Buffer = new object[0];
        }
        else if (count == buffer.Length)
        {
            this.Count = count;
            this.Buffer = buffer;
        }
        else
        {
            this.Count = count;
            object[] final = new object[count];
            Array.Copy(buffer, 0, final, 0, count);
            this.Buffer = final;
        }
    }
}