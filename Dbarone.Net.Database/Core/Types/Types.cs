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

    public static TypeInfo GetByDatabaseType(DatabaseType databaseType)
    {
        return _dict.Values.First(t => t.DatabaseType == databaseType);
    }

    static Types()
    {
        _dict = new Dictionary<Type, TypeInfo> {
            {typeof(bool), new TypeInfo(DatabaseType.Boolean, typeof(bool), 1)},
            {typeof(byte), new TypeInfo(DatabaseType.Byte, typeof(byte), 1)},
            {typeof(sbyte), new TypeInfo(DatabaseType.SByte, typeof(sbyte), 1)},
            {typeof(char), new TypeInfo(DatabaseType.Char, typeof(char), 1)},
            {typeof(decimal), new TypeInfo(DatabaseType.Decimal, typeof(decimal), 16)},
            {typeof(double), new TypeInfo(DatabaseType.Double, typeof(double), 8)},
            {typeof(float), new TypeInfo(DatabaseType.Single, typeof(float), 4)},
            {typeof(Int16), new TypeInfo(DatabaseType.Int16, typeof(Int16), 2)},
            {typeof(UInt16), new TypeInfo(DatabaseType.UInt16, typeof(UInt16), 2)},
            {typeof(Int32), new TypeInfo(DatabaseType.Int32, typeof(Int32), 4)},
            {typeof(UInt32), new TypeInfo(DatabaseType.UInt32, typeof(UInt32), 4)},
            {typeof(Int64), new TypeInfo(DatabaseType.Int64, typeof(Int64), 8)},
            {typeof(UInt64), new TypeInfo(DatabaseType.UInt64, typeof(UInt64), 8)},
            {typeof(Guid), new TypeInfo(DatabaseType.Guid, typeof(Guid), 16)},
            {typeof(DateTime), new TypeInfo(DatabaseType.DateTime, typeof(DateTime), 8)},
            {typeof(string), new TypeInfo(DatabaseType.String, typeof(string), -1)},
            {typeof(byte[]), new TypeInfo(DatabaseType.Blob, typeof(byte[]), -1)}
        };
    }
}
