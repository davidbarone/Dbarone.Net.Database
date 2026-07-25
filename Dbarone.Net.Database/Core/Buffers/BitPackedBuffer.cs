using Dbarone.Net.Database;

/// <summary>
/// Streams in .NET are generally byte-aligned.
/// This class allows for reading and writing
/// of arbitrary numbers of bits in a stream.
/// </summary>
public class BitPackedBuffer : IDisposable
{
  private readonly Stream _stream;
  private int _bitBuffer;       // Holds bits read from the stream
  private int _bitsInBuffer;    // Number of bits currently in the buffer

  public BitPackedBuffer(IBuffer buffer) : this(buffer.Stream) { }

  public BitPackedBuffer(Stream stream)
  {
    _stream = stream ?? throw new ArgumentNullException(nameof(stream));
    if (!stream.CanRead)
      throw new ArgumentException("Stream must be readable.", nameof(stream));
  }

  /// <summary>
  /// Reads an unsigned integer value from the stream using the specified number of bits.
  /// </summary>
  public uint Read(int bitCount)
  {
    if (bitCount <= 0 || bitCount > 32)
      throw new ArgumentOutOfRangeException(nameof(bitCount), "Bit count must be between 1 and 32.");

    uint result = 0;
    int bitsNeeded = bitCount;

    while (bitsNeeded > 0)
    {
      // If buffer is empty, read the next byte
      if (_bitsInBuffer == 0)
      {
        int nextByte = _stream.ReadByte();
        if (nextByte == -1)
          throw new EndOfStreamException("Not enough bits in stream.");

        _bitBuffer = nextByte;
        _bitsInBuffer = 8;
      }

      // Take as many bits as possible from the buffer
      int bitsToTake = Math.Min(bitsNeeded, _bitsInBuffer);
      int shift = _bitsInBuffer - bitsToTake;
      int extractedBits = (_bitBuffer >> shift) & ((1 << bitsToTake) - 1);

      result = (result << bitsToTake) | (uint)extractedBits;

      _bitsInBuffer -= bitsToTake;
      _bitBuffer &= (1 << _bitsInBuffer) - 1; // Mask remaining bits
      bitsNeeded -= bitsToTake;
    }
    return result;
  }

  public void Dispose()
  {
    _stream?.Dispose();
  }
}