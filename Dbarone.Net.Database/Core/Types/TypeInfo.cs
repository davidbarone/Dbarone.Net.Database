public class TypeInfo {

    public DatabaseType DatabaseType { get; set; }
    public Type Type { get; set; }
    public int Size { get; set; }

    public TypeInfo(DatabaseType databaseType, Type type, int size) {
        this.Size = size;
        this.Type = type;
        this.DatabaseType = databaseType;
    }
}