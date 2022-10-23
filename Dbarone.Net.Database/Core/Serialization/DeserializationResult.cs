using Dbarone.Net.Database;

/// <summary>
/// The result of a deserialization, including the row status flags.
/// </summary>
/// <typeparam name="T">The type of object being deserialised into.</typeparam>
public class DeserializationResult<T> {
    public T? Result { get; set; }
    public RowStatus RowStatus { get; set; }

    public DeserializationResult(T? result, RowStatus rowStatus){
        this.Result = result;
        this.RowStatus = rowStatus;
    }
}