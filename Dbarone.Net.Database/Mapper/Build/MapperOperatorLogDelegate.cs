using Dbarone.Net.Database.Mapper;

/// <summary>
/// Defines a callback function which is used for logging purposes.
/// </summary>
/// <param name="mapperOperator">The current mapper operator that is the source of the logging activity.</param>
/// <param name="logType">The log type.</param>
public delegate void MapperOperatorLogDelegate(MapperOperator mapperOperator, MapperOperatorLogType logType);