namespace Dbarone.Net.Database.Parquet;

public class ParquetModel
{
  public FileMetaData MetaData { get; set; } = default!;
  public Table Data { get; set; } = default!;
}