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
using Dbarone.Net.Extensions;

/// <summary>
/// To test Parquet serialization module, we use the Parquet.NET
/// library (https://www.nuget.org/packages/Parquet.Net) as a
// validation tool.
/// </summary>
public class ParquetSerializerTests
{
  [Theory]
  [MemberData(nameof(GetTestDatasets))]
  public async Task ParquetReadTest(string name, List<Dictionary<string, object?>> data)
  {
    Assert.NotNull(name);

    // Write test data to in-memory Parquet using Parquet.NET
    var bytes = await WriteParquetNet(data);

    // Read the parquet ms using both Parquet.NET and Dbarone.Net.Database
    var readParquetNet = await ReadParquetNet(bytes);
    var readParquetDbarone = new ParquetSerializer().Read(bytes);

    if (readParquetNet is null)
    {
      Assert.Fail("readParquetNet should not be null!");
    }
    else
    {
      var md = readParquetNet.Metadata!;
      // Assertions / tests
      Assert.Equal(md.CreatedBy, readParquetDbarone.MetaData.CreatedBy);
      Assert.Equal(md.NumRows, readParquetDbarone.MetaData.NumRows);
      Assert.Equal(md.RowGroups.Count, readParquetDbarone.MetaData.RowGroups.Count);
      Assert.Equal(md.RowGroups[0].TotalByteSize, readParquetDbarone.MetaData.RowGroups[0].TotalByteSize);
      Assert.Equal(md.Schema.Count, readParquetDbarone.MetaData.Schema.Count);
      Assert.Equivalent(md.Schema.Select(s => s.Name), readParquetDbarone.MetaData.Schema.Select(s => s.Name));

      // Test that the original dataset, and the dataset read by Dbarone.Net.Database are the same:
      Assert.Equal(data, readParquetDbarone.Data.ToDictionaryEnumerable(), new DictionaryComparer());
    }
  }

  /// <summary>
  /// Generates the test datasets. Note that this method can be modified to return
  /// only a single dataset by entering the name of the dataset in the parameter.
  /// </summary>
  /// <param name="selectedDataset"></param>
  /// <returns></returns>
  public static IEnumerable<object[]> GetTestDatasets(string? selectedDataset = null)
  {
    // Get test datasets
    var results = new Dictionary<string, List<Dictionary<string, object?>>>();

    // Add test datasets
    results["Int32 1-5"] = GenerateSingleColumnDataset("foo", () => Enumerable.Range(0, 5));

    // Reformat results into IEnumerable<object[]>
    foreach (var key in results.Keys)
    {
      yield return new object[] { key, results[key] };
    }
  }

  #region Private helper methods

  private static List<Dictionary<string, object?>> GenerateSingleColumnDataset<T>(string columnName, Func<IEnumerable<T>> generator)
  {
    var results = new List<Dictionary<string, object?>>();
    var data = generator();
    foreach (var row in data)
    {
      var dict = new Dictionary<string, object?>();
      dict[columnName] = (object?)row;
    }
    return results;
  }

  /// <summary>
  /// Reads data in Parquet.NET object and returns to dictionary list.
  /// </summary>
  /// <param name="reader"></param>
  /// <returns></returns>
  private static async Task<List<Dictionary<string, object>>> ParquetNetToDictionaryList(ParquetReader reader)
  {
    var result = new List<Dictionary<string, object>>();

    for (int g = 0; g < reader.RowGroupCount; g++)
    {
      using (ParquetRowGroupReader groupReader = reader.OpenRowGroupReader(g))
      {
        var fields = reader.Schema.GetDataFields();
        var columns = new List<Parquet.Data.DataColumn>();
        var dataAsList = new List<IList<object>>();

        foreach (var field in fields)
        {
          var col = await groupReader.ReadColumnAsync(field);
          columns.Add(col);
          var list = col.Data.Cast<object>().ToList();
          dataAsList.Add(list);
        }

        int rowCount = columns[0].Data.Length;

        for (int row = 0; row < rowCount; row++)
        {
          var dict = new Dictionary<string, object>();
          for (int col = 0; col < fields.Length; col++)
          {
            dict[fields[col].Name] = dataAsList[col][row] ?? DBNull.Value;
          }
          result.Add(dict);
        }
      }
    }
    return result;
  }

  /// <summary>
  /// Reads a CSV string, where the data type information is included in the header and
  /// returns a dictionary list.
  /// </summary>
  /// <param name="csvData"></param>
  /// <returns></returns>
  private static List<Dictionary<string, object?>> GetDataset(string csvData)
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

  private static byte[] MemoryStreamToByteArray(MemoryStream ms)
  {
    if (ms == null)
      throw new Exception("MemoryStream cannot be null.");

    // Ensure the position is at the beginning
    if (ms.CanSeek)
      ms.Position = 0;

    return ms.ToArray(); // Creates a copy of the data    
  }

  /// <summary>
  /// Takes a dataset (dictionary list) and generates a Parquet.NET file.
  /// This is used as baseline in tests.
  /// </summary>
  /// <param name="rows"></param>
  /// <returns></returns>
  private static async Task<byte[]> WriteParquetNet(List<Dictionary<string, object?>> rows)
  {
    // create schema
    var schema = new ParquetSchema(
      new DataField<int>("foo")
    );

    MemoryStream ms = new MemoryStream();
    using (var parquetWriter = await ParquetWriter.CreateAsync(schema, ms))
    {
      parquetWriter.CompressionMethod = CompressionMethod.None; // default is snappy
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

  private static async Task<ParquetReader> ReadParquetNet(byte[] bytes)
  {
    using (var ms = new MemoryStream(bytes))
    {
      using (ParquetReader reader = await ParquetReader.CreateAsync(ms))
      {
        return reader;
      }
    }
  }

  #endregion

}
