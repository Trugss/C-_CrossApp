using Core.Configuration;
using Microsoft.Extensions.Configuration;

public sealed class AppSettings(IConfigurationRoot root, AppOptions options, string environmentName, string baseDirectory)
{
    public IConfigurationRoot Root { get; } = root;
    public AppOptions Options { get; } = options;
    public string EnvironmentName { get; } = environmentName;
    public string BaseDirectory { get; } = baseDirectory;

    public string? ApiKey => Root["Integration:ApiKey"];

    public string DataFile() => Options.Storage.ResolveFullPath(BaseDirectory);

    public string ApiKeyStatus() =>
        string.IsNullOrEmpty(ApiKey) ? "ApiKey: не заданий" : $"ApiKey: присутній, довжина {ApiKey.Length}";

    public string SourceOf(string key)
    {
        foreach (var provider in Root.Providers.Reverse())
        {
            if (!provider.TryGet(key, out _))
                continue;

            return provider switch
            {
                Microsoft.Extensions.Configuration.Json.JsonConfigurationProvider json
                    when json.Source.Path == "secrets.json" => "user-secrets",
                Microsoft.Extensions.Configuration.Json.JsonConfigurationProvider json => json.Source.Path ?? "json",
                Microsoft.Extensions.Configuration.EnvironmentVariables.EnvironmentVariablesConfigurationProvider =>
                    $"env {AppConfiguration.Prefix}{key.Replace(":", "__")}",
                Microsoft.Extensions.Configuration.Memory.MemoryConfigurationProvider => $"env {AppConfiguration.LegacyDataPathVariable}",
                Microsoft.Extensions.Configuration.CommandLine.CommandLineConfigurationProvider => "командний рядок",
                _ => provider.GetType().Name
            };
        }

        return "типове значення";
    }
}
