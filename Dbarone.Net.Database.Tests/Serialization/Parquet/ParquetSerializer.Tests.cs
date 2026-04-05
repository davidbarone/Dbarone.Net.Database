using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Parquet;
using Parquet.Schema;
using Xunit;
using System;
using Dbarone.Net.Database;

/// <summary>
/// To test Parquet serialization module, we use the Parquet.NET
/// library (https://www.nuget.org/packages/Parquet.Net) as a
// validation tool.
/// </summary>
public class ParquetSerializerTests
{
  public byte[] MemoryStreamToByteArray(MemoryStream ms)
  {
    if (ms == null)
      throw new Exception("MemoryStream cannot be null.");

    // Ensure the position is at the beginning
    if (ms.CanSeek)
      ms.Position = 0;

    return ms.ToArray(); // Creates a copy of the data    
  }

  private async Task<byte[]> Dataset1()
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
    using FileStream fs = new FileStream("test.parquet", FileMode.Create, FileAccess.Write);
    ms.WriteTo(fs);
    return MemoryStreamToByteArray(ms);
  }

  [Fact]
  public async Task SerializeTableNoSchemaTest()
  {
    var bytes = await Dataset1();
    GenericBuffer buf = new GenericBuffer(bytes);
    ParquetSerializer ser = new ParquetSerializer();
    ser.Deserialize(buf);

  }
}