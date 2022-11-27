namespace Dbarone.Net.Database;
using System;
using System.Collections.Generic;
using Dbarone.Net.Extensions.Reflection;
using Dbarone.Net.Assertions;
using System.Text;

/// <summary>
/// Defines the types allowed in Dbarone.Net.Database.
/// </summary>
public class Types
{
    private static Dictionary<Type, TypeInfo> _dict { get; set; } = new Dictionary<Type, TypeInfo>();

    /// <summary>
    /// Gets the TypeInfo for a given .NET type.
    /// </summary>
    /// <param name="type">The .NET type</param>
    /// <returns>The corresponding TypeInfo value.</returns>
    public static TypeInfo GetByType(Type type)
    {
        var t = type;
        if (type.IsEnum)
        {
            t = Enum.GetUnderlyingType(type);
        }
        else if (type.IsNullable())
        {
            t = type.GetNullableUnderlyingType();
        }
        if (t == null)
        {
            throw new Exception($"Cannot get type information for type: [${type.Name}].");
        }
        return _dict[t];
    }

    public static TypeInfo GetByDataType(DataType dataType)
    {
        return _dict.Values.First(t => t.DataType == dataType);
    }

    /// <summary>
    /// Gets the actual size in bytes of an object.
    /// </summary>
    /// <param name="obj">The object to determine the size of.</param>
    /// <returns></returns>
    public static int SizeOf(object obj, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        if (obj == null) { return 0; }
        var typeInfo = GetByType(obj.GetType());
        if (typeInfo.IsFixedLength)
        {
            return typeInfo.Size;
        }
        else if (typeInfo.DataType == DataType.String)
        {
            if (textEncoding == TextEncoding.UTF8)
            {
                return (int)Encoding.UTF8.GetBytes((string)obj).Length;
            }
            else if (textEncoding == TextEncoding.Latin1)
            {
                return (int)Encoding.Latin1.GetBytes((string)obj).Length;
            }
            else
            {
                throw new Exception("Unable to get SizeOf due to invalid text encoding.");
            }
        }
        else if (typeInfo.DataType == DataType.Blob)
        {
            return (int)((byte[])obj).Length;
        }
        else
        {
            throw new Exception($"Invalid object type: {obj.GetType().Name}");
        }
    }

    static Types()
    {
        _dict = new Dictionary<Type, TypeInfo> {
            {typeof(bool), new TypeInfo(DataType.Boolean, typeof(bool), 1)},
            {typeof(byte), new TypeInfo(DataType.Byte, typeof(byte), 1)},
            {typeof(sbyte), new TypeInfo(DataType.SByte, typeof(sbyte), 1)},
            {typeof(char), new TypeInfo(DataType.Char, typeof(char), 2)},
            {typeof(decimal), new TypeInfo(DataType.Decimal, typeof(decimal), 16)},
            {typeof(double), new TypeInfo(DataType.Double, typeof(double), 8)},
            {typeof(float), new TypeInfo(DataType.Single, typeof(float), 4)},
            {typeof(Int16), new TypeInfo(DataType.Int16, typeof(Int16), 2)},
            {typeof(UInt16), new TypeInfo(DataType.UInt16, typeof(UInt16), 2)},
            {typeof(Int32), new TypeInfo(DataType.Int32, typeof(Int32), 4)},
            {typeof(UInt32), new TypeInfo(DataType.UInt32, typeof(UInt32), 4)},
            {typeof(Int64), new TypeInfo(DataType.Int64, typeof(Int64), 8)},
            {typeof(UInt64), new TypeInfo(DataType.UInt64, typeof(UInt64), 8)},
            {typeof(Guid), new TypeInfo(DataType.Guid, typeof(Guid), 16)},
            {typeof(DateTime), new TypeInfo(DataType.DateTime, typeof(DateTime), 8)},
            {typeof(string), new TypeInfo(DataType.String, typeof(string), 0)},
            {typeof(byte[]), new TypeInfo(DataType.Blob, typeof(byte[]), 0)}
        };
    }

    /// <summary>
    /// Gets the serial type of a value
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static SerialType GetSerialType(object? obj, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        if (obj == null)
        {
            return new SerialType(DataType.Null);
        }

        var objType = obj.GetType();

        if (objType == typeof(string))
        {
            // Serial type for string values is (N-variableTypeStart)/2 and odd. 
            var length = Types.SizeOf(obj, textEncoding);
            return new SerialType(DataType.String, length);
        }
        else if (objType == typeof(byte[]))
        {
            // Serial type for byte[] values is (N-variableTypeStart)/2 and even. 
            var length = Types.SizeOf(obj, textEncoding);
            return new SerialType(DataType.Blob, length);
        }
        else
        {
            // For other types, return a VarInt of the DataType enum value.
            var dataType = Types.GetByType(objType).DataType;
            return new SerialType(dataType);
        }
    }
}
