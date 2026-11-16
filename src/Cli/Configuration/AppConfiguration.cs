using System.Reflection;
using Core.Configuration;
using Microsoft.Extensions.Configuration;

public static class AppConfiguration
{
    public const string Prefix = "CROSSAPP_";
    public const string EnvironmentVariable = "CROSSAPP_ENVIRONMENT";
    public const string LegacyDataPathVariable = "CROSSAPP_DATA_PATH";

    private static readonly Dictionary<string, string> SwitchMappings = new()
    {
        ["--data-path"] = "Storage:DataPath",
        ["--file-name"] = "Storage:FileName",
        ["--format"] = "Storage:Format"
    };

    public static AppSettings Load(string[] args)
    {
        var environmentName = Environment.GetEnvironmentVariable(EnvironmentVariable);
        if (string.IsNullOrWhiteSpace(environmentName))
            environmentName = "Development";

        var legacyDataPath = Environment.GetEnvironmentVariable(LegacyDataPathVariable);
        var legacy = new Dictionary<string, string?>();
        if (!string.IsNullOrWhiteSpace(legacyDataPath))
            legacy["Storage:DataPath"] = legacyDataPath;

        var baseDirectory = AppContext.BaseDirectory;
        var root = new ConfigurationBuilder()
            .SetBasePath(baseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: false)
            .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
            .AddEnvironmentVariables(prefix: Prefix)
            .AddInMemoryCollection(legacy)
            .AddCommandLine(args.Where(a => a.Contains('=')).ToArray(), SwitchMappings)
            .Build();

        var options = root.Get<AppOptions>() ?? new AppOptions();
        return new AppSettings(root, options, environmentName, baseDirectory);
    }
}
