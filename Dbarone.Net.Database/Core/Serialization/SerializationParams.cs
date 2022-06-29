public class ColumnSerializationInfo
{
    public string ColumnName { get; set; } = default!;
    public object? Value { get; set; }
    public short Size { get; set; }
}

/// <summary>
/// Parameters in relation to serialization.
/// </summary>
public class SerializationParams
{
    public short TotalCount { get; set; }
    public short TotalSize { get; set; }
    public short FixedCount { get; set; }
    public short FixedSize { get; set; }
    public List<ColumnSerializationInfo> FixedColumns { get; set; } = default!;
    public short VariableCount { get; set; }
    public short VariableSize { get; set; }
    public List<ColumnSerializationInfo> VariableColumns { get; set; } = default!;
}