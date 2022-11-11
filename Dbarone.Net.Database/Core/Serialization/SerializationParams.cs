public class ColumnSerializationInfo
{
    /// <summary>
    /// The column name.
    /// </summary>
    public string ColumnName { get; set; } = default!;

    /// <summary>
    /// The column value.
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// The size of the serialised value in bytes.
    /// </summary>
    public int Size { get; set; }
}

/// <summary>
/// Parameters in relation to serialization.
/// </summary>
public class SerializationParams
{
    /// <summary>
    /// Total number of fixed-length columns (max 255).
    /// </summary>
    public byte FixedCount { get; set; }

    /// <summary>
    /// Total size of fixed-length columns (max length 65536).
    /// </summary>
    public ushort FixedSize { get; set; }

    /// <summary>
    /// The fixed length column information.
    /// </summary>
    public List<ColumnSerializationInfo> FixedColumns { get; set; } = default!;

    /// <summary>
    /// Total number of variable-length columns (max 255).
    /// </summary>
    public byte VariableCount { get; set; }

    /// <summary>
    /// Size of variable-length data.
    /// </summary>
    public int VariableSize { get; set; }

    /// <summary>
    /// Variable length column information.
    /// </summary>
    public List<ColumnSerializationInfo> VariableColumns { get; set; } = default!;

    /// <summary>
    /// Total column count (read only)
    /// </summary>
    public byte TotalCount => (byte)(FixedCount + VariableCount);

    /// <summary>
    /// Total data size (read only)
    /// </summary>
    public int TotalSize => (int)(VariableSize + (int)FixedSize);
}