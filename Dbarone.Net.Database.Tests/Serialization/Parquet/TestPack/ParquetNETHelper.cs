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
/// Provides helper functions for reading / writing using Parquet.NET.
/// Parquet.NET is used as reference / baseline for testing.
/// </summary>
public class ParquetNETHelper
{
  /// <summary>
  /// Generates an in-memory Parquet file using a test pack table.
  /// </summary>
  /// <param name="table">The source test pack table.</param>
  /// <returns></returns>
  public static async Task<byte[]> CreateFromTestPackTable(TestPackTable table)
  {
    var rows = table.GenerateEnumerableDictionary();

    // create schema
    List<Field> fields = new List<Field>();
    foreach (var item in table.Keys)
    {
      var name = item;
      var dataType = table[item].DataType;
      switch (dataType)
      {
        case Type _ when dataType == typeof(Int32):
          fields.Add(new DataField<int>(name));
          break;
        case Type _ when dataType == typeof(Int64):
          fields.Add(new DataField<long>(name));
          break;
      }
    }
    var schema = new ParquetSchema(fields);

    MemoryStream ms = new MemoryStream();
    using (var parquetWriter = await ParquetWriter.CreateAsync(schema, ms))
    {
      parquetWriter.CompressionMethod = CompressionMethod.None; // default is snappy
      using (ParquetRowGroupWriter groupWriter = parquetWriter.CreateRowGroup())
      {
        foreach (var field in schema.Fields)
        {
          var dataField = field as DataField;
          if (dataField is not null)
          {
            switch (dataField.ClrType)
            {
              case Type _ when dataField.ClrType == typeof(Int32):
                await groupWriter
                  .WriteColumnAsync(
                    new Parquet.Data.DataColumn((DataField)field,
                    rows.Select(r => Convert.ToInt32(r[field.Name])).ToArray()));
                break;
              case Type _ when dataField.ClrType == typeof(Int64):
                await groupWriter
                  .WriteColumnAsync(
                    new Parquet.Data.DataColumn((DataField)field,
                    rows.Select(r => Convert.ToInt64(r[field.Name])).ToArray()));
                break;
              default:
                throw new Exception($"Cannot write {dataField.ClrType} type.");
            }
          }
        }
      }
    }
    using FileStream fs = new FileStream("test.parquet", FileMode.Create, FileAccess.Write);
    ms.WriteTo(fs);
    return MemoryStreamToByteArray(ms);
  }

  public static async Task<ParquetReader> Read(byte[] bytes)
  {
    using (var ms = new MemoryStream(bytes))
    {
      using (ParquetReader reader = await ParquetReader.CreateAsync(ms))
      {
        return reader;
      }
    }
  }

  /// <summary>
  /// Reads data in Parquet.NET object and returns to dictionary list.
  /// </summary>
  /// <param name="reader"></param>
  /// <returns></returns>
  public static async Task<List<Dictionary<string, object?>>> ToEnumerableDictionary(ParquetReader reader)
  {
    var result = new List<Dictionary<string, object?>>();

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
          var dict = new Dictionary<string, object?>();
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

  #region Private methods

  private static byte[] MemoryStreamToByteArray(MemoryStream ms)
  {
    if (ms == null)
      throw new Exception("MemoryStream cannot be null.");

    // Ensure the position is at the beginning
    if (ms.CanSeek)
      ms.Position = 0;

    return ms.ToArray(); // Creates a copy of the data    
  }

  #endregion

}