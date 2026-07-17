using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Parquet;
using Parquet.Schema;
using Xunit;
using System;
using Dbarone.Net.Database;
using Dbarone.Net.Csv;
using System.Linq;
using Dbarone.Net.Database.Tests;

/// <summary>
/// To test Parquet serialization module, we use the Parquet.NET
/// library (https://www.nuget.org/packages/Parquet.Net) as a
// validation tool.
/// </summary>
public class ParquetSerializerTests
{
  #region Datasets

  /// <summary>
  /// Single foo column (integer), and 5 rows
  /// </summary>
  private string dataset1 => @"foo:int
1
2
3
4
5";

  #endregion

  private List<Dictionary<string, object?>> GetDataset(string csvData)
  {
    var encoding = System.Text.Encoding.UTF8;
    byte[] byteArray = encoding.GetBytes(csvData ?? string.Empty);
    var ms = new MemoryStream(byteArray);
    CsvReader reader = new CsvReader(ms);

    // The column names have the data types. Cast here
    List<Dictionary<string, object?>> results = new List<Dictionary<string, object?>>();
    foreach (var row in reader.Read().ToList())
    {
      Dictionary<string, object?> dict = new Dictionary<string, object?>();
      foreach (var key in row.Keys)
      {
        var name_type = key.Split(":");
        var column_name = name_type[0];
        var dataType = name_type[1];
        switch (dataType.ToLower())
        {
          case "int":
            dict[column_name] = Convert.ToInt32(row[key]);
            break;
          default:
            dict[column_name] = null;
            break;
        }
      }
      results.Add(dict);
    }
    return results;
  }

  public byte[] MemoryStreamToByteArray(MemoryStream ms)
  {
    if (ms == null)
      throw new Exception("MemoryStream cannot be null.");

    // Ensure the position is at the beginning
    if (ms.CanSeek)
      ms.Position = 0;

    return ms.ToArray(); // Creates a copy of the data    
  }

  private async Task<byte[]> WriteParquetNet(List<Dictionary<string, object?>> rows)
  {
    // create schema
    var schema = new ParquetSchema(
      new DataField<int>("foo")
    );

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
            columnData.Add(Convert.ToInt32(row[field.Name]));
          }

          await groupWriter.WriteColumnAsync(new Parquet.Data.DataColumn((DataField)field, columnData.ToArray()));
        }
      }
    }
    using FileStream fs = new FileStream("test.parquet", FileMode.Create, FileAccess.Write);
    ms.WriteTo(fs);
    return MemoryStreamToByteArray(ms);
  }


  private async Task<ParquetReader> ReadParquetNet(byte[] bytes)
  {
    using (var ms = new MemoryStream(bytes))
    {
      using (ParquetReader reader = await ParquetReader.CreateAsync(ms))
      {
        return reader;
      }
    }
  }

  [Fact]
  public async Task ParquetReadTest()
  {
    var csv = dataset1;
    var data = GetDataset(csv);

    // Write to in-memory Parquet using Parquet.NET
    var bytes = await WriteParquetNet(data);

    // Read the parquet ms using both Parquet.NET and Dbarone.Net.Database
    var readParquetNet = ReadParquetNet(bytes);
    var readParquetDbarone = new ParquetSerializer().Read(bytes);

    // Assertions / tests
    Assert.Equal(readParquetNet.Result.Metadata.CreatedBy, readParquetDbarone.MetaData.CreatedBy);
    Assert.Equal(readParquetNet.Result.Metadata.NumRows, readParquetDbarone.MetaData.NumRows);
    Assert.Equal(readParquetNet.Result.Metadata.RowGroups[0].TotalByteSize, readParquetDbarone.MetaData.RowGroups[0].TotalByteSize);


  }

  /*   [Fact]
    public async Task SerializeTableNoSchemaTest()
    {
      var bytes = await Dataset1();
      GenericBuffer buf = new GenericBuffer(bytes);
      ParquetSerializer ser = new ParquetSerializer();
      ser.Deserialize(buf);

    }
   */
}