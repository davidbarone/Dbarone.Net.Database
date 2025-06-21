namespace Dbarone.Net.Database;

/// <summary>
/// Encodes the data type and size of a value.
/// 
/// When serialising values, the payload is prefixed with a
/// SerialType value, which encodes the value's data type
/// and length.
/// 
/// The values are as follows:
/// 
/// Fixed Width DocumentTypes
/// =========================
///
/// Serial Type     Document Type
/// -----------     -------------
/// 0               NULL
/// 1               Boolean
/// 2               Integer
/// 3               Real
/// 4               DateTime
/// 
/// Variable Length data types
/// ========================== 
/// 
/// Serial Type     Document Type
/// -----------     -------------
/// N>=6,  N%2==0   Blob. Value is a byte array that is (N-6)/2 bytes long.
/// N>=7,  N%2==1   String. Value is a string that is (N-7)/2 bytes long, stored in the text encoding of the database.
/// </summary>
public class SerialType
{
    /// <summary>
    /// The DocumentType of the value.
    /// </summary>
    public DocumentType DocumentType { get; set; }

    /// <summary>
    /// Byte length of the data if string or blob.
    /// </summary>
    public int? Length { get; set; }

    public VarInt Value { get; init; }

    private const int VariableStart = (int)DocumentType.Blob;

    public SerialType(VarInt value)
    {
        if (value.Value < VariableStart)
        {
            // Fixed-width type
            this.DocumentType = (DocumentType)value.Value;
            this.Length = null;
        }
        else if (value.Value % 2 == 0)
        {
            // blob
            this.DocumentType = DocumentType.Blob;
            this.Length = ((int)value.Value - ((int)value.Value % 2) - VariableStart) / 2;
        }
        else if (value.Value % 2 == 1)
        {
            // text
            this.DocumentType = DocumentType.Text;
            this.Length = ((int)value.Value - ((int)value.Value % 2) - VariableStart) / 2;
        }
    }

    public SerialType(DocumentType DocumentType, int? length = null)
    {
        this.DocumentType = DocumentType;
        this.Length = length;

        if (DocumentType == DocumentType.Blob)
        {
            if (length == null)
            {
                throw new Exception("Length must be set.");
            }
            this.Value = VariableStart + (length.Value * 2);
        }
        else if (DocumentType == DocumentType.Text)
        {
            if (length == null)
            {
                throw new Exception("Length must be set.");
            }
            this.Value = VariableStart + (length.Value * 2) + 1;
        }
        else if ((int)DocumentType < VariableStart)
        {
            this.Value = (int)DocumentType;
        }
        else
        {
            throw new Exception("Should not get here!");
        }
    }
}