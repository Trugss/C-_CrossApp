using Core;
using Core.Services;
using Core.Storage;

public static class SettingsCommands
{
    public static readonly string[] Commands = ["config", "export", "load"];

    private static readonly string[] Keys =
    [
        "EnvironmentName", "Logging:MinimumLevel", "Storage:DataPath", "Storage:FileName",
        "Storage:Format", "Storage:UseLocalAppData", "Library:LoanDays", "Integration:ApiKey"
    ];

    public static int Run(string command, AppSettings settings, ConsoleLog log, LendingService service, string[] args) =>
        command switch
        {
            "config" => Config(settings),
            "export" => Export(settings, log, service),
            _ => Load(settings, log, args)
        };

    private static int Config(AppSettings settings)
    {
        var options = settings.Options;
        Console.WriteLine($"Профіль             : {settings.EnvironmentName}");
        Console.WriteLine($"CurrentDirectory    : {Environment.CurrentDirectory}");
        Console.WriteLine($"BaseDirectory       : {settings.BaseDirectory}");
        Console.WriteLine($"LocalApplicationData: {Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}");
        Console.WriteLine($"Файл даних          : {settings.DataFile()}");
        Console.WriteLine($"Рівень логу         : {options.Logging.MinimumLevel}");
        Console.WriteLine($"Строк видачі        : {options.Library.LoanDays} дн.");
        Console.WriteLine(settings.ApiKeyStatus());
        Console.WriteLine();
        Console.WriteLine($"{"Ключ",-24} {"Значення",-14} Джерело");
        foreach (var key in Keys)
        {
            var value = settings.Root[key];
            var shown = key == "Integration:ApiKey" && value is not null ? "***" : value ?? "-";
            Console.WriteLine($"{key,-24} {shown,-14} {settings.SourceOf(key)}");
        }

        return 0;
    }

    private static int Export(AppSettings settings, ConsoleLog log, LendingService service)
    {
        var dataFile = settings.DataFile();
        Console.WriteLine($"Файл даних : {dataFile}");
        Console.WriteLine(settings.ApiKeyStatus());

        var exporter = CatalogExporters.Create(settings.Options.Storage.Format);
        var target = CatalogExporters.TargetPath(exporter, dataFile);
        log.Debug($"Експортер {exporter.GetType().Name}, запис через {Path.GetFileName(target)}.tmp і File.Move");

        var count = exporter.Export(service.All(), target);
        Console.WriteLine($"Експортовано {count} {Records(count)} у форматі {exporter.Format}.");
        Console.WriteLine($"{Path.GetFileName(target)}:");
        foreach (var line in File.ReadLines(target).Take(exporter.Format == "json" ? 8 : 4))
            Console.WriteLine($"   {line}");

        return 0;
    }

    private static int Load(AppSettings settings, ConsoleLog log, string[] args)
    {
        var i = Array.IndexOf(args, "--path");
        var path = i >= 0 && i + 1 < args.Length ? Path.GetFullPath(args[i + 1]) : settings.DataFile();
        Console.WriteLine($"Файл даних : {path}");

        if (!File.Exists(path))
        {
            log.Warn("Файл ще не створено, спочатку виконайте export.");
            return 1;
        }

        try
        {
            var store = new FileBookStore(path);
            var copies = store.List();
            Console.WriteLine($"Імпортовано з JSON {copies.Count} {Records(copies.Count)}.");
            foreach (var c in copies.Take(5))
                Console.WriteLine($"   {c.Id,-8} {c.Isbn,-20} {c.Title,-32} {c.Genre}");
            return 0;
        }
        catch (StorageException ex)
        {
            Console.WriteLine($"Технічна помилка: {ex.Message}");
            log.Debug($"InnerException: {ex.InnerException?.GetType().Name}");
            return 1;
        }
        catch (DomainException ex)
        {
            Console.WriteLine($"Дані відхилено: {ex.Message}");
            return 1;
        }
    }

    private static string Records(int count) =>
        (count % 10, count % 100) switch
        {
            (1, not 11) => "запис",
            (>= 2 and <= 4, < 12 or > 14) => "записи",
            _ => "записів"
        };
}
