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
/// Generates a column for a test pack table.
/// </summary>
public class TestPackColumn
{
  public TestPackColumn(Type dataType, Func<IEnumerable<object>> generator)
  {
    this.DataType = dataType;
    this.Generator = generator;
  }

  public Type DataType { get; set; } = default!;
  public Func<IEnumerable<object>> Generator { get; set; } = default!;
}