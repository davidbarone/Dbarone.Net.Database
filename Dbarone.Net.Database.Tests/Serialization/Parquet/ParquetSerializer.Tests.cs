using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Parquet;
using Parquet.Schema;
using Xunit;

/// <summary>
/// To test Parquet serialization module, we use the Parquet.NET
/// library (https://www.nuget.org/packages/Parquet.Net) as a
// validation tool.
/// </summary>
public class ParquetSerializerTests
{

  private async Task<Stream> Dataset1()
  {
    var schema = new ParquetSchema(
      new DataField<int>("foo")
    );

    var rows = new List<Dictionary<string, int>>
    {
      new Dictionary<string, int> { ["foo"] = 1 },
      new Dictionary<string, int> { ["foo"] = 2 },
      new Dictionary<string, int> { ["foo"] = 3 },
      new Dictionary<string, int> { ["foo"] = 4 },
      new Dictionary<string, int> { ["foo"] = 5 }
    };

    MemoryStream ms = new MemoryStream();
    using (var parquetWriter = await ParquetWriter.CreateAsync(schema, ms))
    {
      using (ParquetRowGroupWriter groupWriter = parquetWriter.CreateRowGroup())
      {
        foreach (var field in schema.Fields)
        {
          var columnData = new List<int>();
          foreach (var row in rows)
          {
            columnData.Add(row[field.Name]);
          }

          await groupWriter.WriteColumnAsync(new Parquet.Data.DataColumn((DataField)field, columnData.ToArray()));
        }
      }
    }
    return ms;
  }

  [Fact]
  public async Task SerializeTableNoSchemaTest()
  {
    var parquet = await Dataset1();
    var a = parquet.Length;
  }
}