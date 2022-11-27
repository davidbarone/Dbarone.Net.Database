using Dbarone.Net.Database;

/// <summary>
/// Encodes the data type and length of a data body value.
/// 
/// The serial type is stored in the database as a VarInt. The values are as follows:
/// 
/// Serial Type     Data Type
/// ===========     =========
/// 0               NULL value
/// 1               Boolean
/// 2               Byte
/// 3               SByte
/// 4               Char
/// 5               Decimal
/// 6               Double
/// 7               Single
/// 8               VarInt
/// 9               Int16
/// 10              UInt16
/// 11              Int32
/// 12              UInt32
/// 13              Int64
/// 14              UInt64
/// 15              DateTime
/// 16              Guid
/// 17              Array
/// N>=18 and even  Blob. Value is a byte array that is (N-18)/2 bytes long.
/// N>=19 and odd   String. Value is a string that is (N-19)/2 bytes long, stored in the text encoding of the database.
/// </summary>
public class SerialType
{
    /// <summary>
    /// Data type of the value.
    /// </summary>
    public DataType DataType { get; set; }

    /// <summary>
    /// Byte length of the data if string or blob.
    /// </summary>
    public int? Length { get; set; }

    public VarInt Value { get; init; }

    public SerialType(VarInt value)
    {
        int variableTypeStart = (int)DataType.Blob;
        int evenStart = variableTypeStart % 2 == 1 ? variableTypeStart + 1 : variableTypeStart;
        int oddStart = evenStart + 1;

        if (value.Value < variableTypeStart)
        {
            this.DataType = (DataType)value.Value;
            this.Length = null;
        }
        else if (value.Value % 2 == 0)
        {
            // even number = blob
            this.DataType = DataType.Blob;
            this.Length = (value.Value - evenStart) / 2;
        }
        else
        {
            // odd number = string
            this.DataType = DataType.String;
            this.Length = (value.Value - oddStart) / 2;
        }
    }

    public SerialType(DataType dataType, int? length = null)
    {
        this.DataType = dataType;
        this.Length = length;

        int variableTypeStart = (int)DataType.Blob;
        int evenStart = variableTypeStart % 2 == 1 ? variableTypeStart + 1 : variableTypeStart;
        int oddStart = evenStart + 1;

        if (dataType == DataType.Blob)
        {
            if (length == null)
            {
                throw new Exception("Length must be set.");
            }
            // Serial type for byte[] values is (N-variableTypeStart)/2 and even. 
            this.Value = new VarInt((length.Value * 2) + evenStart);
        }
        else if (dataType == DataType.String)
        {
            if (length == null)
            {
                throw new Exception("Length must be set.");
            }
            // Serial type for string values is (N-variableTypeStart)/2 and odd. 
            this.Value = new VarInt((length.Value * 2) + oddStart);
        }
        else
        {
            // For other types, return a VarInt of the DataType enum value.
            this.Value = new VarInt((int)dataType);
        }
    }
}