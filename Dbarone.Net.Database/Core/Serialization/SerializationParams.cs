public class ColumnSerializationInfo
{
    public string ColumnName { get; set; } = default!;
    public object? Value { get; set; }
    public ushort Size { get; set; }
}

/// <summary>
/// Parameters in relation to serialization.
/// </summary>
public class SerializationParams
{
    public ushort TotalCount { get; set; }
    public ushort TotalSize { get; set; }
    public ushort FixedCount { get; set; }
    public ushort FixedSize { get; set; }
    public List<ColumnSerializationInfo> FixedColumns { get; set; } = default!;
    public ushort VariableCount { get; set; }
    public ushort VariableSize { get; set; }
    public List<ColumnSerializationInfo> VariableColumns { get; set; } = default!;
}