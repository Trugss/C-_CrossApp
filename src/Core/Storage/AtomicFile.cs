namespace Core.Storage;

public static class AtomicFile
{
    public static void WriteAllText(string path, string content)
    {
        var fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var tmp = fullPath + ".tmp";
        File.WriteAllText(tmp, content);
        File.Move(tmp, fullPath, overwrite: true);
    }
}
