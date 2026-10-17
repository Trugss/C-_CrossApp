using Core.Abstractions;
using Core.Events;

public sealed class FileSink : INotificationSink
{
    public FileSink(string path)
    {
        FilePath = path;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    }

    public string FilePath { get; }

    public void Notify(LibraryChangedEventArgs change)
    {
        try
        {
            File.AppendAllText(FilePath, ChangeFormatter.Format(change) + Environment.NewLine);
        }
        catch (IOException ex)
        {
            Console.WriteLine($"   [sink] не вдалося записати лог: {ex.Message}");
        }
    }
}
