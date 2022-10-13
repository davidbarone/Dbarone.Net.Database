namespace Dbarone.Net.Database;

public class MetadataItemKey {
    public Type PageType { get; set; }
    public int? ObjectId { get; set; }
}

public class MetadataItemValue {
    public Type HeaderType { get; set; }
    public Type DataElementType { get; set; }
    public IEnumerable<ColumnInfo> DataElementColumns { get; set; }
    

}

/// <summary>
/// Services to provide metadata information for objects.
/// </summary>
public class MetdataManager
{
    
}