namespace Core.Configuration;

public sealed class LoggingOptions
{
    public AppLogLevel MinimumLevel { get; init; } = AppLogLevel.Information;
}
