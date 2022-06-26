namespace Dbarone.Net.Database;
using System;
using System.Collections.Generic;

/// <summary>
/// Defines the types allowed in Dbarone.Net.Database.
/// </summary>
public class Types
{
    private static Dictionary<Type, TypeInfo> _dict { get; set; } = new Dictionary<Type, TypeInfo>();

    public static TypeInfo GetByType(Type type)
    {
        return _dict[type];
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
    public static short SizeOf(object obj)
    {
        var typeInfo = GetByType(obj.GetType());
        if (typeInfo.IsFixedLength){
            return typeInfo.Size;
        } else if (typeInfo.DataType==DataType.String){
            return (short)((string)obj).Length;
        } else if (typeInfo.DataType==DataType.Blob){
            return (short)((byte[])obj).Length;
        } else {
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
            {typeof(string), new TypeInfo(DataType.String, typeof(string), -1)},
            {typeof(byte[]), new TypeInfo(DataType.Blob, typeof(byte[]), -1)}
        };
    }
}
