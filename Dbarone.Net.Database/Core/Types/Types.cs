namespace Dbarone.Net.Database;
using System;
using System.Collections.Generic;

/// <summary>
/// Defines the types allowed in Dbarone.Net.Database.
/// </summary>
public class Types
{
    private static List<Type> fixedLengthTypes;
    private static List<Type> variableLengthTypes;
    private static Dictionary<Type, TypeInfo> typesDict { get; set; } = default!;

    public static Dictionary<Type, TypeInfo> Get()
    {
        return typesDict;
    }

    public static Types()
    {
        fixedLengthTypes = new List<Type>() {

            // Value Types
            typeof(bool),
            typeof(byte),
            typeof(sbyte),
            typeof(char),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(short),
            typeof(ushort),

            // Reference Types
            typeof(DateTime)
        };

        variableLengthTypes = new List<TypeInfo>{
            // Reference Types
            typeof(string)
        };

        foreach (var item in fixedLengthTypes)
        {
            var databaseType = Enum.Parse<DatabaseType>(item.Name);
            typesDict[item] = new TypeInfo(databaseType, item, System.Runtime.InteropServices.Marshal.SizeOf(item));
        }

        foreach (var item in variableLengthTypes)
        {
            var databaseType = Enum.Parse<DatabaseType>(item.Name);
            typesDict[item] = new TypeInfo(databaseType, item, -1);
        }
    }
}
