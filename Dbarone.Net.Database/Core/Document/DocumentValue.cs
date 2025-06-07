using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Dbarone.Net.Database;

/// <summary>
/// Represent a simple value used in Document.
/// </summary>
public class DocumentValue : IComparable<DocumentValue>, IEquatable<DocumentValue>
{
    /// <summary>
    /// Represents a Null type.
    /// </summary>
    public static DocumentValue Null = new DocumentValue();

    /// <summary>
    /// Indicate DataType of this value.
    /// </summary>
    public DocumentType Type { get; }

    /// <summary>
    /// Get internal .NET value object.
    /// </summary>
    public virtual object? RawValue { get; }

    #region Constructors

    public DocumentValue()
    {
        this.Type = DocumentType.Null;
        this.RawValue = null;
    }

    public DocumentValue(Int64 value)
    {
        this.Type = DocumentType.Integer;
        this.RawValue = value;
    }

    public DocumentValue(Double value)
    {
        this.Type = DocumentType.Real;
        this.RawValue = value;
    }

    public DocumentValue(String value)
    {
        this.Type = value == null ? DocumentType.Null : DocumentType.Text;
        this.RawValue = value;
    }

    public DocumentValue(Byte[] value)
    {
        this.Type = value == null ? DocumentType.Null : DocumentType.Blob;
        this.RawValue = value;
    }

    public DocumentValue(object? value)
    {
        this.RawValue = value;

        if (value == null) this.Type = DocumentType.Null;
        else if (value is Int64) this.Type = DocumentType.Integer;
        else if (value is Double) this.Type = DocumentType.Real;
        else if (value is String) this.Type = DocumentType.Text;
        else if (value is Byte[]) this.Type = DocumentType.Blob;
    }

    #endregion

    #region Convert types

    public Byte[] AsBlob => this.RawValue as Byte[];

    public string AsText => (string)this.RawValue;

    public Int64 AsInteger => Convert.ToInt64(this.RawValue);

    public double AsReal => Convert.ToDouble(this.RawValue);

    #endregion

    #region IsTypes

    public bool IsNull => this.Type == DocumentType.Null;

    public bool IsInteger => this.Type == DocumentType.Integer;

    public bool IsReal => this.Type == DocumentType.Real;

    public bool IsBlob => this.Type == DocumentType.Blob;

    public bool IsNumber => this.IsInteger || this.IsReal;

    public bool IsText => this.Type == DocumentType.Text;

    #endregion

    #region Implicit Ctor

    // Boolean
    public static implicit operator Boolean(DocumentValue value)
    {
        return value.AsInteger == 0 ? false : true;
    }

    // Boolean?
    public static implicit operator Boolean?(DocumentValue value)
    {
        return value.Type == DocumentType.Null ? null : value.AsInteger == 0 ? false : true;
    }

    // Boolean
    public static implicit operator DocumentValue(Boolean value)
    {
        return new DocumentValue(value ? 1 : 0);
    }

    // Boolean?
    public static implicit operator DocumentValue(Boolean? value)
    {
        return (value is null) ? new DocumentValue() : new DocumentValue(value.Value ? 1 : 0);
    }

    // Byte
    public static implicit operator Byte(DocumentValue value)
    {
        return (Byte)value.AsInteger;
    }

    // Byte?
    public static implicit operator Byte?(DocumentValue value)
    {
        return value.Type == DocumentType.Null ? null : (Byte)value.AsInteger;
    }

    // Byte
    public static implicit operator DocumentValue(Byte value)
    {
        return new DocumentValue(value);
    }

    // Byte?
    public static implicit operator DocumentValue(Byte? value)
    {
        return new DocumentValue(value);
    }

    // SByte
    public static implicit operator SByte(DocumentValue value)
    {
        return (SByte)value.AsInteger;
    }

    // SByte?
    public static implicit operator SByte?(DocumentValue value)
    {
        return value.Type == DocumentType.Null ? null : (SByte)value.AsInteger;
    }

    // SByte
    public static implicit operator DocumentValue(SByte value)
    {
        return new DocumentValue(value);
    }

    // SByte?
    public static implicit operator DocumentValue(SByte? value)
    {
        return new DocumentValue(value);
    }

    // Char
    public static implicit operator Char(DocumentValue value)
    {
        return (Char)value.AsInteger;
    }

    // Char?
    public static implicit operator Char?(DocumentValue value)
    {
        return value.Type == DocumentType.Null ? null : (Char)value.AsInteger;
    }

    // Char
    public static implicit operator DocumentValue(Char value)
    {
        return new DocumentValue(value);
    }

    // Char?
    public static implicit operator DocumentValue(Char? value)
    {
        return new DocumentValue(value);
    }

    // Int16
    public static implicit operator Int16(DocumentValue value)
    {
        return (Int16)value.AsInteger;
    }

    // Int16?
    public static implicit operator Int16?(DocumentValue value)
    {
        return value.Type == DocumentType.Null ? null : (Int16)value.AsInteger;
    }

    // Int16
    public static implicit operator DocumentValue(Int16 value)
    {
        return new DocumentValue(value);
    }

    // Int16?
    public static implicit operator DocumentValue(Int16? value)
    {
        return new DocumentValue(value);
    }

    // UInt16
    public static implicit operator UInt16(DocumentValue value)
    {
        return (UInt16)value.AsInteger;
    }

    // UInt16?
    public static implicit operator UInt16?(DocumentValue value)
    {
        return value.Type == DocumentType.Null ? null : (UInt16)value.AsInteger;
    }

    // UInt16
    public static implicit operator DocumentValue(UInt16 value)
    {
        return new DocumentValue(value);
    }

    // UInt16?
    public static implicit operator DocumentValue(UInt16? value)
    {
        return new DocumentValue(value);
    }

    // Int32
    public static implicit operator Int32(DocumentValue value)
    {
        return (Int32)value.AsInteger;
    }

    // Int32?
    public static implicit operator Int32?(DocumentValue value)
    {
        return value.Type == DocumentType.Null ? null : (Int32)value.AsInteger;
    }

    // Int32
    public static implicit operator DocumentValue(Int32 value)
    {
        return new DocumentValue(value);
    }

    // Int32?
    public static implicit operator DocumentValue(Int32? value)
    {
        return new DocumentValue(value);
    }

    // UInt32
    public static implicit operator UInt32(DocumentValue value)
    {
        return (UInt32)value.AsInteger;
    }

    // UInt32?
    public static implicit operator UInt32?(DocumentValue value)
    {
        return value.Type == DocumentType.Null ? null : (UInt32)value.AsInteger;
    }

    // UInt32
    public static implicit operator DocumentValue(UInt32 value)
    {
        return new DocumentValue(value);
    }

    // UInt32?
    public static implicit operator DocumentValue(UInt32? value)
    {
        return new DocumentValue(value);
    }

    // Int64
    public static implicit operator Int64(DocumentValue value)
    {
        return (Int64)value.AsInteger;
    }

    // Int64?
    public static implicit operator Int64?(DocumentValue value)
    {
        return value.Type == DocumentType.Null ? null : (Int64)value.AsInteger;
    }

    // Int64
    public static implicit operator DocumentValue(Int64 value)
    {
        return new DocumentValue(value);
    }

    // Int64?
    public static implicit operator DocumentValue(Int64? value)
    {
        return new DocumentValue(value);
    }

    // UInt64
    public static implicit operator UInt64(DocumentValue value)
    {
        return (UInt64)value.AsInteger;
    }

    // UInt64?
    public static implicit operator UInt64?(DocumentValue value)
    {
        return value.Type == DocumentType.Null ? null : (UInt64)value.AsInteger;
    }

    // UInt64
    public static implicit operator DocumentValue(UInt64 value)
    {
        return new DocumentValue(value);
    }

    // UInt64?
    public static implicit operator DocumentValue(UInt64? value)
    {
        return new DocumentValue(value);
    }

    // Single
    public static implicit operator Single(DocumentValue value)
    {
        return (Single)value.AsReal;
    }

    // Single?
    public static implicit operator Single?(DocumentValue value)
    {
        return value.Type == DocumentType.Null ? null : (Single)value.AsReal;
    }

    // Single
    public static implicit operator DocumentValue(Single value)
    {
        return new DocumentValue(value);
    }

    // Single?
    public static implicit operator DocumentValue(Single? value)
    {
        return new DocumentValue(value);
    }


    // DateTime
    public static implicit operator DateTime(DocumentValue value)
    {
        return DateTime.FromBinary(value.AsInteger);
    }

    // DateTime?
    public static implicit operator DateTime?(DocumentValue value)
    {
        return value.Type == DocumentType.Null ? null : DateTime.FromBinary(value.AsInteger);
    }

    // DateTime
    public static implicit operator DocumentValue(DateTime value)
    {
        return new DocumentValue(value);
    }

    // DateTime?
    public static implicit operator DocumentValue(DateTime? value)
    {
        return new DocumentValue(value);
    }

    // Double
    public static implicit operator Double(DocumentValue value)
    {
        return (Double)value.AsReal;
    }

    // Double?
    public static implicit operator Double?(DocumentValue value)
    {
        return value.Type == DocumentType.Null ? null : (Double)value.AsReal;
    }

    // Double
    public static implicit operator DocumentValue(Double value)
    {
        return new DocumentValue(value);
    }

    // Double?
    public static implicit operator DocumentValue(Double? value)
    {
        return new DocumentValue(value);
    }

    // Decimal
    public static implicit operator Decimal(DocumentValue value)
    {
        return (Decimal)value.AsReal;
    }

    // Decimal?
    public static implicit operator Decimal?(DocumentValue value)
    {
        return value.Type == DocumentType.Null ? null : (Decimal)value.AsReal;
    }

    // Decimal
    public static implicit operator DocumentValue(Decimal value)
    {
        return new DocumentValue(value);
    }

    // Decimal?
    public static implicit operator DocumentValue(Decimal? value)
    {
        return new DocumentValue(value);
    }

    // Guid
    public static implicit operator Guid(DocumentValue value)
    {
        return new Guid(value.AsBlob);
    }

    // Guid?
    public static implicit operator Guid?(DocumentValue value)
    {
        return value.Type == DocumentType.Null ? null : new Guid(value.AsBlob);
    }

    // Guid
    public static implicit operator DocumentValue(Guid value)
    {
        return new DocumentValue(value);
    }

    // Guid?
    public static implicit operator DocumentValue(Guid? value)
    {
        return new DocumentValue(value);
    }

    // String
    public static implicit operator String(DocumentValue value)
    {
        return value.AsText;
    }

    // String
    public static implicit operator DocumentValue(String value)
    {
        return new DocumentValue(value);
    }

    // Binary
    public static implicit operator Byte[](DocumentValue value)
    {
        return value.AsBlob;
    }

    // Binary
    public static implicit operator DocumentValue(Byte[] value)
    {
        return new DocumentValue(value);
    }

    // +
    public static DocumentValue operator +(DocumentValue left, DocumentValue right)
    {
        if (!left.IsNumber || !right.IsNumber) return DocumentValue.Null;
        if (left.IsInteger && right.IsNull) return left.AsInteger + right.AsInteger;
        else return left.AsReal + right.AsReal;
    }

    // -
    public static DocumentValue operator -(DocumentValue left, DocumentValue right)
    {
        if (!left.IsNumber || !right.IsNumber) return DocumentValue.Null;
        if (left.IsInteger && right.IsNull) return left.AsInteger - right.AsInteger;
        else return left.AsReal - right.AsReal;
    }

    // *
    public static DocumentValue operator *(DocumentValue left, DocumentValue right)
    {
        if (!left.IsNumber || !right.IsNumber) return DocumentValue.Null;
        if (left.IsInteger && right.IsNull) return left.AsInteger * right.AsInteger;
        else return left.AsReal * right.AsReal;
    }

    // /
    public static DocumentValue operator /(DocumentValue left, DocumentValue right)
    {
        if (!left.IsNumber || !right.IsNumber) return DocumentValue.Null;
        if (left.IsInteger && right.IsNull) return left.AsInteger / right.AsInteger;
        else return left.AsReal / right.AsReal;
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }

    #endregion

    #region IComparable, IEquatable

    public virtual int CompareTo(DocumentValue other)
    {
        return this.CompareTo(other, Collation.Binary);
    }

    public virtual int CompareTo(DocumentValue other, Collation collation)
    {
        // first, test if types are different
        if (this.Type != other.Type)
        {
            // if both values are number, convert them to Decimal (128 bits) to compare
            // it's the slowest way, but more secure
            if (this.IsNumber && other.IsNumber)
            {
                return Convert.ToDecimal(this.RawValue).CompareTo(Convert.ToDecimal(other.RawValue));
            }
            // if not, order by sort type order
            else
            {
                var result = this.Type.CompareTo(other.Type);
                return result < 0 ? -1 : result > 0 ? +1 : 0;
            }
        }

        // for both values with same data type just compare
        switch (this.Type)
        {
            case DocumentType.Null:
                return 0;

            case DocumentType.Int32: return this.AsInt32.CompareTo(other.AsInt32);
            case DocumentType.Int64: return this.AsInt64.CompareTo(other.AsInt64);
            case DocumentType.Double: return this.AsDouble.CompareTo(other.AsDouble);
            case DocumentType.Decimal: return this.AsDecimal.CompareTo(other.AsDecimal);

            case DocumentType.String: return collation.Compare(this.AsString, other.AsString);

            case DocumentType.Document: return this.AsDocument.CompareTo(other);
            case DocumentType.Array: return this.AsArray.CompareTo(other);

            case DocumentType.Blob: return this.BinaryCompare(this.AsBinary, other.AsBinary);
            case DocumentType.Guid: return this.AsGuid.CompareTo(other.AsGuid);

            case DocumentType.Boolean: return this.AsBoolean.CompareTo(other.AsBoolean);
            case DocumentType.DateTime:
                var d0 = this.AsDateTime;
                var d1 = other.AsDateTime;
                if (d0.Kind != DateTimeKind.Utc) d0 = d0.ToUniversalTime();
                if (d1.Kind != DateTimeKind.Utc) d1 = d1.ToUniversalTime();
                return d0.CompareTo(d1);

            default: throw new NotImplementedException();
        }
    }

    private int BinaryCompare(byte[] lh, byte[] rh)
    {
        if (lh == null) return rh == null ? 0 : -1;
        if (rh == null) return 1;

        var result = 0;
        var i = 0;
        var stop = Math.Min(lh.Length, rh.Length);

        for (; 0 == result && i < stop; i++)
            result = lh[i].CompareTo(rh[i]);

        if (result != 0) return result < 0 ? -1 : 1;
        if (i == lh.Length) return i == rh.Length ? 0 : -1;
        return 1;
    }

    public bool Equals(DocumentValue other)
    {
        return this.CompareTo(other) == 0;
    }

    #endregion

    #region Operators

    public static bool operator ==(DocumentValue lhs, DocumentValue rhs)
    {
        if (object.ReferenceEquals(lhs, null)) return object.ReferenceEquals(rhs, null);
        if (object.ReferenceEquals(rhs, null)) return false; // don't check type because sometimes different types can be ==

        return lhs.Equals(rhs);
    }

    public static bool operator !=(DocumentValue lhs, DocumentValue rhs)
    {
        return !(lhs == rhs);
    }

    public static bool operator >=(DocumentValue lhs, DocumentValue rhs)
    {
        return lhs.CompareTo(rhs) >= 0;
    }

    public static bool operator >(DocumentValue lhs, DocumentValue rhs)
    {
        return lhs.CompareTo(rhs) > 0;
    }

    public static bool operator <(DocumentValue lhs, DocumentValue rhs)
    {
        return lhs.CompareTo(rhs) < 0;
    }

    public static bool operator <=(DocumentValue lhs, DocumentValue rhs)
    {
        return lhs.CompareTo(rhs) <= 0;
    }

    public override bool Equals(object obj)
    {
        if (obj is DocumentValue other)
        {
            return this.Equals(other);
        }

        return false;
    }

    public override int GetHashCode()
    {
        var hash = 17;
        hash = 37 * hash + this.Type.GetHashCode();
        hash = 37 * hash + (this.RawValue?.GetHashCode() ?? 0);
        return hash;
    }

    #endregion

    #region GetBytesCount()

    /// <summary>
    /// Returns how many bytes this BsonValue will consume when converted into binary BSON
    /// If recalc = false, use cached length value (from Array/Document only)
    /// </summary>
    /// <param name="recalc">Set to true to force recalculation.</param>
    /// <returns>Returns the document length.</returns>
    /// <exception cref="ArgumentException">Throws an exception if an invalid document type.</exception>
    internal virtual int GetBytesCount(bool recalc)
    {
        switch (this.Type)
        {
            case DocumentType.Null: return 0;

            case DocumentType.Boolean:
            case DocumentType.SByte:
            case DocumentType.Byte: return 1;

            case DocumentType.Char:
            case DocumentType.Int16:
            case DocumentType.UInt16: return 2;

            case DocumentType.Int32:
            case DocumentType.UInt32:
            case DocumentType.Single: return 4;

            case DocumentType.Int64:
            case DocumentType.UInt64:
            case DocumentType.DateTime:
            case DocumentType.Double: return 8;

            case DocumentType.Decimal:
            case DocumentType.Guid: return 16;

            case DocumentType.String: return Encoding.UTF8.GetByteCount(this.AsString);
            case DocumentType.Blob: return this.AsBinary.Length;
            case DocumentType.Document: return this.AsDocument.GetBytesCount(recalc);
            case DocumentType.Array: return this.AsArray.GetBytesCount(recalc);
        }

        throw new ArgumentException();
    }

    /// <summary>
    /// Get how many bytes one single element will used in BSON format
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns>Returns the byte count for an element.</returns>
    protected int GetBytesCountElement(string key, DocumentValue value)
    {
        // check if data type is variant
        var variant = value.Type == DocumentType.String || value.Type == DocumentType.Blob || value.Type == DocumentType.Guid;

        return
            1 + // element type
            Encoding.UTF8.GetByteCount(key) + // CString
            1 + // CString \0
            value.GetBytesCount(true) +
            (variant ? 5 : 0); // bytes.Length + 0x??
    }

    #endregion

    #region Document Schema

    #endregion

}
