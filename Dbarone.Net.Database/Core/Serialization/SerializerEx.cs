namespace Dbarone.Net.Database;
using Dbarone.Net.Assertions;
using System.Collections;
using Dbarone.Net.Extensions.Object;
using System.Collections.Generic;

/// <summary>
/// An enhanced serialier, similar in function to the Sqlite serialiser.
/// </summary>
public class SerializerEx : SerializerBase
{
    /// <summary>
    /// Format as follows:
    /// VarInt: Total Serialised Size
    /// VarInt: Header Size
    /// 
    /// Header:
    /// - VarInt: bytes in the header
    /// - VarInt[]: One or more VarInts (one per column)
    ///
    /// Body:
    /// - Zero or more values
    /// 
    /// </summary>
    /// <param name="columns"></param>
    /// <param name="obj"></param>
    /// <param name="rowStatus"></param>
    /// <param name="textEncoding"></param>
    /// <returns></returns>
    public override byte[] SerializeDictionary(IEnumerable<ColumnInfo> columns, IDictionary<string, object?> obj, RowStatus rowStatus, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        
        return base.SerializeDictionary(columns, obj, rowStatus, textEncoding);
    }

    public override DeserializationResult<IDictionary<string, object?>> DeserializeDictionary(IEnumerable<ColumnInfo> columns, byte[] buffer, TextEncoding textEncoding = TextEncoding.UTF8)
    {
        return base.DeserializeDictionary(columns, buffer, textEncoding);
    }
}
