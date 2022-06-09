public class Types
{
    private List<Type> types;
    private static Dictionary<Type, TypeInfo> typesDict { get; set; } = default!;

    public static Dictionary<Type, TypeInfo> Get()
    {
        return typesDict;
    }

    public Types()
    {
        types = new List<Type>() {

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
            typeof(DateTime),
            typeof(string)
        };

        foreach (var item in types)
        {
            typesDict[item] = new TypeInfo(DatabaseType.TYPE_BOOL, item, System.Runtime.InteropServices.Marshal.SizeOf(item));
        }
    }
}