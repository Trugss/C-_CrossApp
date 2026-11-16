using Core.Configuration;

public sealed class ConsoleLog(AppLogLevel minimum)
{
    public AppLogLevel Minimum { get; } = minimum;

    public void Debug(string message) => Write(AppLogLevel.Debug, "dbg", message);
    public void Info(string message) => Write(AppLogLevel.Information, "inf", message);
    public void Warn(string message) => Write(AppLogLevel.Warning, "wrn", message);
    public void Error(string message) => Write(AppLogLevel.Error, "err", message);

    private void Write(AppLogLevel level, string tag, string message)
    {
        if (level >= Minimum)
            Console.WriteLine($"[{tag}] {message}");
    }
}
