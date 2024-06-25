using Dbarone.Net.Database;

/// <summary>
/// The header entry in a page that references overflow data.
/// </summary>
public class OverflowPointer
{
    public int FirstOverflowPageId { get; set; }
    public int BufferSize { get; set; }

    public OverflowPointer(int firstOverflowPageId, int bufferSize)
    {
        this.FirstOverflowPageId = firstOverflowPageId;
        this.BufferSize = bufferSize;
    }

    public OverflowPointer() { }
}