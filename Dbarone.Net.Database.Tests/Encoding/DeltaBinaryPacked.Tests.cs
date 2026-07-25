using Xunit;
using Dbarone.Net.Database;
using System;
using System.Collections.Generic;

namespace Dbarone.Net.Document.Tests;

public class DeltaBinaryPackedTests
{
  [Theory]
  [InlineData(new byte[] { 128, 8, 32, 5, 2, 2, 0 }, new int[] { 1, 2, 3, 4, 5 })]
  public void TestDecode(byte[] encodedBytes, int[] expectedValues)
  {
    var dbp = new DeltaBinaryPacked();
    GenericBuffer buffer = new GenericBuffer(encodedBytes);
    dbp.Decode(buffer);
  }
}