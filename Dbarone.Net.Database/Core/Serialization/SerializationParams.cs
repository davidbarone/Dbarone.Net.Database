/// <summary>
/// Parameters in relation to serialization.
/// </summary>
public class SerializationParams {
    public short TotalCount { get; set; }
    public short TotalLength { get; set; }
    public short FixedCount { get; set; }
    public short FixedSize { get; set; }
    public short VariableCount { get; set; }
    public IDictionary<string, short> VariableSizes { get; set; } = default!;
}