namespace Dbarone.Net.Database;

/// <summary>
/// Provides compression for signed integers, and uses VarInt encoding internally.
/// </summary>
/// <remarks>
/// ZigZag compression works by converting signed integers to unsigned integers
/// then using VarInt encoding.
/// - https://lemire.me/blog/2022/11/25/making-all-your-integers-positive-with-zigzag-encoding/
/// - https://curioloop.com/en/posts/variable-length-numeric-compression
/// </remarks>
public class ZigZag
{
    public static int SizeOf(long value)
    {
        ZigZag zz = new ZigZag(value);
        return zz.VarInt.Size;
    }

    public ZigZag(long value)
    {
        int sizeLong = sizeof(long);
        Decoded = value;
        Encoded = (ulong)((Decoded << 1) ^ (Decoded >> (sizeLong * 8 - 1)));
        VarInt = Encoded;
    }

    public ZigZag(VarInt varInt)
    {
        this.VarInt = varInt;
        Encoded = this.VarInt.Value;
        Decoded = ((long)Encoded >> 1) ^ -((long)Encoded & 1);
    }

    public ulong Encoded { get; set; }

    public long Decoded { get; set; }

    public VarInt VarInt { get; set; }
}