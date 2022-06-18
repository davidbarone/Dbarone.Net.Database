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
    private static Dictionary<Type, TypeInfo> typesDict { get; set; } = new Dictionary<Type, TypeInfo>();

    public static TypeInfo GetByType(Type type)
    {
        return typesDict[type];
    }

    public static TypeInfo GetByDatabaseType(DatabaseType databaseType)
    {
        return typesDict.Values.First(t => t.DatabaseType == databaseType);
    }

    static Types()
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

        variableLengthTypes = new List<Type>{
            // Reference Types
            typeof(string)
        };

        foreach (var item in fixedLengthTypes)
        {
            var databaseType = Enum.Parse<DatabaseType>(item.Name, true);
            var size = 0;
            if (item==typeof(DateTime)){
                size = System.Runtime.InteropServices.Marshal.SizeOf(DateTime.Now.ToBinary());
            } else {
                size = System.Runtime.InteropServices.Marshal.SizeOf(item);
            }
            typesDict[item] = new TypeInfo(databaseType, item, size);
        }

        foreach (var item in variableLengthTypes)
        {
            var databaseType = Enum.Parse<DatabaseType>(item.Name);
            typesDict[item] = new TypeInfo(databaseType, item, -1);
        }
    }
}
