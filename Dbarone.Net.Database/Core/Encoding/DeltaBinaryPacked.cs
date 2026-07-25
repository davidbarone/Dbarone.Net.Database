using System.ComponentModel.DataAnnotations;
using Dbarone.Net.Database;

/// <summary>
/// Implementation of delta binary packed encoding used in Parquet
/// 
/// Used to compress INT32 and INT64
/// https://parquet.apache.org/docs/file-format/data-pages/encodings/#DELTAENC
/// Parquet adaption from: https://arxiv.org/pdf/1209.2137v5
/// 
/// Delta encoding consists of a header followed by blocks of delta encoded
/// values, binary packed. Each block is made up of mini blocks, each of
/// them packed with its own bit width.
/// 
/// Header is defined as:
/// <block size in values> <number of miniblocks in a block> <total value count> <first value>
/// where:
/// - the block size is a multiple of 128; it is stored as a ULEB128 int
/// - the miniblock count per block is a divisor of the block size such that their quotient, the number of values in a miniblock, is a multiple of 32; it is stored as a ULEB128 int
/// - the total value count is stored as a ULEB128 int
/// - the first value is stored as a zigzag ULEB128 int
/// 
/// Each block contains:
/// <min delta> <list of bitwidths of miniblocks> <miniblocks>
/// where:
/// the min delta is a zigzag ULEB128 int (we compute a minimum as we need positive integers for bit packing)
/// the bitwidth of each miniblock is stored as a byte
/// each miniblock is a list of bit-packed ints according to the bit width stored at the beginning of the block
/// </summary>
public class DeltaBinaryPacked
{
  /// <summary>
  /// Decodes to an sequence of long integers.
  /// </summary>
  /// <param name="buffer"></param>
  /// <returns></returns>
  public IEnumerable<long> Decode(IBuffer buffer)
  {
    // Block size (ULEB128)
    var blockSize = buffer.ReadVarInt(Endianness.LITTLE_ENDIAN);
    // Number of mini blocks (ULEB128)
    var miniblockCount = buffer.ReadVarInt(Endianness.LITTLE_ENDIAN);
    // Total values (ULEB128)
    var totalValues = buffer.ReadVarInt(Endianness.LITTLE_ENDIAN);
    // First value (zigzag ULEB128)
    var firstValue = new ZigZag(buffer.ReadVarInt(Endianness.LITTLE_ENDIAN));
    // valuesInMiniBlock (calculated: must be multiple of 32)
    var valuesInMiniBlock = blockSize / miniblockCount;

    var processed = 0;
    var blockCount = totalValues / blockSize + 1;
    for (int i = 0; i < blockCount; i++)
    {
      // calculate how many miniblocks in this block:
      var miniBlocksInBlock = (totalValues - (i * blockSize)) / valuesInMiniBlock + 1;
      if (miniBlocksInBlock <= 0 || miniBlocksInBlock > miniblockCount)
      {
        throw new Exception('Invalid miniBlocksInBlock!');
      }

      // process each block
      // Min Delta (zigzag ULEB128)
      var minDelta = new ZigZag(buffer.ReadVarInt(Endianness.LITTLE_ENDIAN));
      // Read in the bit-width (byte) for EACH mini block in block
      List<byte> bitWidths = new List<byte>();
      for (int j = 0; j < miniBlocksInBlock; j++)
      {
        bitWidths.Add(buffer.ReadBytes(1)[0]);
      }
      // read each miniblock





    }
    while (i < blockSize)
    {
      // read next miniblock

    }

  }
}