using Dbarone.Net.Database;

public class ParquetSerializer
{
  /// <summary>
  /// Deserializes a buffer contains parquet-formatted data, into a table.
  /// </summary>
  /// <param name="buffer"></param>
  /// <param name="textEncoding"></param>
  /// <returns></returns>
  public Table Deserialize(IBuffer buffer, TextEncoding textEncoding = TextEncoding.UTF8)
  {
    return new Table();
  }
}