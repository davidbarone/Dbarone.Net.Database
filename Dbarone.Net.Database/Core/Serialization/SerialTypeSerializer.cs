namespace Dbarone.Net.Database;
using Dbarone.Net.Assertions;
using System.Collections;
using Dbarone.Net.Extensions.Object;
using System.Collections.Generic;

/// <summary>
/// A serialier that uses a SerialType structure to encode values.
/// 
/// The serialised format is as follows:
/// 
/// SerialisedSize (VarInt):    Total Serialised Size including this field
/// RowStatus (Byte):           RowStatus
/// 
/// Header Section:
/// Header Size (VarInt):       Total bytes in the header section (including this field)
/// SerialTypes (VarInt[]):     One or more VarInts (one per column) to define the column types and lengths
///
/// Body Section:
/// Values (Byte[][]):          Array of byte[] values representing the serialised values
/// </summary>
public class SerialTypeSerializer : BaseSerializer, ISerializer
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="columns"></param>
    /// <param name="obj"></param>
    /// <param name="rowStatus"></param>
    /// <param name="textEncoding"></param>
    /// <returns></returns>
    public override byte[] SerializeDictionary(IEnumerable<ColumnInfo> columns, IDictionary<string, object?> obj, RowStatus rowStatus, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        BufferBase buffer = new BufferBase(new byte[0] { });
        int i = 0;

        if (rowStatus == RowStatus.Deleted || rowStatus == RowStatus.Null)
        {
            var headerLength = new VarInt(0);
            var totalLength = new VarInt(headerLength.Length + Types.GetByType(typeof(RowStatus)).Size); // this length fixed
            var bufferLength = new VarInt(totalLength.Value);
            var overflow = bufferLength.Add(bufferLength.Length);
            if (overflow)
            {
                bufferLength.Add(1);
            }
            // Add the buffer length field to the total length
            overflow = totalLength.Add(bufferLength.Length);
            if (overflow)
            {
                overflow = bufferLength.Add(1);
                if (overflow)
                {
                    bufferLength.Add(1);
                }
            }
            buffer = new BufferBase(new byte[bufferLength.Value]);
            buffer.Write(bufferLength.Bytes, i);
            i = i + bufferLength.Length;
            buffer.Write(rowStatus, i);
            i = i + Types.GetByDataType(DataType.Byte).Size;
            buffer.Write(headerLength.Bytes, i);
            i = i + headerLength.Length;

            // Finish - check length OK
            Assert.Equals(bufferLength.Value, i);
        }
        else
        {
            // header values
            List<SerialType> serialTypes = new List<SerialType>();
            // body values
            List<object?> values = new List<object?>();
            List<int> sizes = new List<int>();

            foreach (var column in columns)
            {
                if (obj.ContainsKey(column.Name))
                {
                    var serialType = Types.GetSerialType(obj[column.Name]);
                    serialTypes.Add(serialType);
                    values.Add(obj[column.Name]);
                    var typeInfo = Types.GetByDataType(serialType.DataType);
                    sizes.Add(typeInfo.IsFixedLength ? typeInfo.Size : serialType.Length!.Value);
                }
                else
                {
                    serialTypes.Add(Types.GetSerialType(null));
                    values.Add(null);
                    sizes.Add(0);
                }
            }

            // Get size of data
            var bodySize = sizes.Sum();

            var headerLength = new VarInt(serialTypes.Sum(s => s.Value.Length));
            var headerVarInt = new VarInt(headerLength.Value);
            var overflow = headerVarInt.Add(headerVarInt.Length);
            if (overflow)
            {
                headerVarInt.Add(1);
            }
            overflow = headerLength.Add(headerVarInt.Length);
            if (overflow)
            {
                overflow = headerVarInt.Add(1);
                if (overflow)
                {
                    headerVarInt.Add(1);
                }
            }
            var totalBufferSize = new VarInt(headerVarInt.Value + bodySize + Types.GetByDataType(DataType.Byte).Size /*RowStatus*/);
            var totalBufferVarInt = new VarInt(totalBufferSize.Value);
            overflow = totalBufferVarInt.Add(totalBufferVarInt.Length);
            if (overflow)
            {
                totalBufferVarInt.Add(1);
            }

            // Can write output now
            buffer = new BufferBase(new byte[totalBufferVarInt.Value]);
            buffer.Write(totalBufferVarInt.Bytes, i);
            i = i + totalBufferVarInt.Length;

            buffer.Write(rowStatus, i);
            i = i + Types.GetByDataType(DataType.Byte).Size;

            buffer.Write(headerVarInt.Bytes, i);
            i = i + headerVarInt.Length;

            foreach (var serialType in serialTypes)
            {
                buffer.Write(serialType.Value.Bytes, i);
                i = i + serialType.Value.Length;
            }

            for (var j = 0; j < values.Count(); j++)
            {
                if (values[j] != null && sizes[j] > 0)
                {
                    buffer.Write(values[j]!, i);
                    i = i + sizes[j];
                }
            }
            // Finish - check length OK
            Assert.Equals(totalBufferVarInt.Value, i);
        }
        return buffer.ToArray();
    }

    public override DeserializationResult<IDictionary<string, object?>> DeserializeDictionary(IEnumerable<ColumnInfo> columns, byte[] buffer, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        var i = 0;
        List<ColumnInfo> columnList = new List<ColumnInfo>(columns);
        Dictionary<string, object?>? obj = null;
        IBuffer bb = new BufferBase(buffer);
        var bufferLength = bb.ReadVarInt(i);
        i = i + bufferLength.Length;
        Assert.Equals(buffer.Length, bufferLength.Value);

        var rowStatus = (RowStatus)bb.ReadByte(i);
        i = i + Types.GetByDataType(DataType.Byte).Size;

        if (rowStatus.HasFlag(RowStatus.Deleted) || rowStatus.HasFlag(RowStatus.Null))
        {
            // no need to deserialize anything.
        }
        else
        {
            obj = new Dictionary<string, object?>();

            // headers
            List<VarInt> serialTypeInts = new List<VarInt>();
            var headerLength = bb.ReadVarInt(i);
            i = i + headerLength.Length;
            while (i < (headerLength.Value + bufferLength.Length + Types.GetByDataType(DataType.Byte).Size))
            {
                var serialTypeInt = bb.ReadVarInt(i);
                i = i + serialTypeInt.Length;
                serialTypeInts.Add(serialTypeInt);
            }

            // body
            for (int j = 0; j < serialTypeInts.Count(); j++)
            {
                var serialTypeInt = serialTypeInts[j];
                var column = columnList[j];
                var serialType = new SerialType(serialTypeInt);
                var typeInfo = Types.GetByDataType(serialType.DataType);
                if (typeInfo.DataType == DataType.Null)
                {
                    obj[column.Name] = null;
                }
                else
                {
                    obj[column.Name] = bb.Read(serialType.DataType, i, serialType.Length);
                }
                i = i + (typeInfo.IsFixedLength ? typeInfo.Size : serialType.Length!.Value);
            }

            // check
            Assert.Equals(bufferLength.Value, i);
        }
        return new DeserializationResult<IDictionary<string, object?>>(obj, rowStatus);
    }

    public override byte[] GetCellBuffer(PageBuffer buffer, int index)
    {
        // First field is the total length (VarInt).
        var totalLength = buffer.ReadVarInt(index);
        var b = buffer.Slice(index, totalLength.Value);
        return b;
    }

    /// <summary>
    /// Inspects a buffer, and extracts the row status.
    /// </summary>
    /// <param name="buffer"></param>
    public override RowStatus GetRowStatus(byte[] buffer)
    {
        var varInt = GetBufferLength(buffer);
        IBuffer bb = new BufferBase(buffer);
        var rowStatus = (RowStatus)bb.ReadByte(varInt.Length);
        return rowStatus;
    }

}
