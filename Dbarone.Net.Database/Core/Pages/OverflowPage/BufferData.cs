using Dbarone.Net.Database;

public class BufferData
{
    public byte[] Buffer { get; set; }
    public BufferData(byte[] buffer)
    {
        this.Buffer = buffer;
    }
}