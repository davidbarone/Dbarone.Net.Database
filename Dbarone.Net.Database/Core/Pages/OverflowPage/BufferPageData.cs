using Dbarone.Net.Database;

public class BufferPageData : IPageData {
    public byte[] Buffer { get; set; }
    public BufferPageData(byte[]buffer){
        this.Buffer = buffer;
    }
}