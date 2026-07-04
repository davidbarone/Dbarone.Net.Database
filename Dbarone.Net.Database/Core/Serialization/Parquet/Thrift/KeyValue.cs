namespace Dbarone.Net.Database.Thrift;

/// <summary>
/// Wrapper struct to store key values
/// </summary>
public sealed class KeyValue
{
  /// <summary>
  /// Field: 1
  /// </summary>
  public string Key { get; set; }

  /// <summary>
  /// Field: 2
  /// </summary>
  public string? Value { get; set; }
}
