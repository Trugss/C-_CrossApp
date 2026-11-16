namespace Core.Configuration;

public sealed class StorageOptions
{
    public const string ApplicationFolder = "CrossApp";

    public string DataPath { get; init; } = "data";
    public string FileName { get; init; } = "library.json";
    public string Format { get; init; } = "json";
    public bool UseLocalAppData { get; init; }

    public string ResolveRoot(string baseDir)
    {
        if (Path.IsPathRooted(DataPath))
            return Path.GetFullPath(DataPath);

        var anchor = UseLocalAppData
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ApplicationFolder)
            : baseDir;

        return Path.GetFullPath(Path.Combine(anchor, DataPath));
    }

    public string ResolveFullPath(string baseDir)
    {
        var root = ResolveRoot(baseDir);
        Directory.CreateDirectory(root);
        return Path.GetFullPath(Path.Combine(root, FileName));
    }
}
