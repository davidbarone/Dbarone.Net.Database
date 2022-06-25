namespace Dbarone.Net.Database.Tests;
using System;

public class TestEntity
{
    public bool BoolProperty { get; set; }
    public byte ByteProperty { get; set; }
    public sbyte SByteProperty { get; set; }
    public char CharProperty { get; set; }
    public decimal DecimalProperty { get; set; }
    public double DoubleProperty { get; set; }
    public Single SingleProperty { get; set; }
    public Int16 Int16Property { get; set; }
    public UInt16 UInt16Property { get; set; }
    public Int32 Int32Property { get; set; }
    public UInt32 UInt32Property { get; set; }
    public Int64 Int64Property { get; set; }
    public UInt64 UInt64Property { get; set; }
    public DateTime DateTimeProperty { get; set; }
    public Guid GuidProperty { get; set; }
    public string StringProperty { get; set; } = default!;
    public byte[] BlobProperty { get; set; } = default!;

    public static TestEntity Create()
    {
        return new TestEntity
        {
            BoolProperty = true,
            ByteProperty = byte.MaxValue,
            SByteProperty = sbyte.MaxValue,
            CharProperty = char.MaxValue,
            DecimalProperty = decimal.MaxValue,
            DoubleProperty = double.MaxValue,
            SingleProperty = Single.MaxValue,
            Int16Property = Int16.MaxValue,
            UInt16Property = UInt16.MaxValue,
            Int32Property = Int32.MaxValue,
            UInt32Property = UInt32.MaxValue,
            Int64Property = Int64.MaxValue,
            UInt64Property = UInt64.MaxValue,
            DateTimeProperty = DateTime.MaxValue,
            StringProperty = "foo bar",
            GuidProperty = Guid.NewGuid(),
            BlobProperty = new byte[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }
        };
    }
}
