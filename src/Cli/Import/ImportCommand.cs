using Core;
using Core.Abstractions;
using Core.Events;
using Core.Services;
using Core.Storage;

public static class ImportCommand
{
    public static async Task RunAsync(LendingService service, string[] args)
    {
        var path = Value(args, "--path") ?? Path.Combine("data", "catalog.json");
        var stream = args.Contains("--stream");
        var delay = Number(args, "--delay");
        var cancelAfter = Number(args, "--cancel-after");
        var timeout = Number(args, "--timeout");

        ICatalogImporter importer = new JsonCatalogImporter();
        if (delay > 0)
            importer = new SlowCatalogImporter(importer, TimeSpan.FromMilliseconds(delay));

        Console.WriteLine($"Файл: {path}");
        Console.WriteLine($"Режим: {(stream ? "потоковий, IAsyncEnumerable + await foreach" : "LoadAsync + IProgress<int>")}" +
                          (delay > 0 ? $", затримка {delay} мс на запис" : "") +
                          (cancelAfter > 0 ? $", CancelAfter {cancelAfter} мс" : "") +
                          (timeout > 0 ? $", таймаут {timeout} мс (зв'язаний токен)" : ""));
        Console.WriteLine($"Примірників у сховищі до імпорту: {service.All().Count}");

        var events = 0;
        EventHandler<LibraryChangedEventArgs> onChanged = (_, e) =>
        {
            events++;
            if (stream)
                Console.WriteLine($"   [подія] {ChangeFormatter.Format(e)}");
        };
        service.Changed += onChanged;

        using var cts = new CancellationTokenSource();
        ConsoleCancelEventHandler onCancelKey = (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };
        Console.CancelKeyPress += onCancelKey;
        if (cancelAfter > 0)
            cts.CancelAfter(TimeSpan.FromMilliseconds(cancelAfter));

        var progress = new ConsoleProgress();
        try
        {
            var added = timeout > 0
                ? await TimeoutRunner.RunAsync(t => Import(service, importer, path, stream, progress, t),
                    TimeSpan.FromMilliseconds(timeout), cts.Token)
                : await Import(service, importer, path, stream, progress, cts.Token);
            progress.Finish();
            Console.WriteLine($"Імпортовано записів: {added}");
        }
        catch (TimeoutException ex)
        {
            progress.Finish();
            Console.WriteLine($"Імпорт скасовано: спрацював таймаут. {ex.Message}");
        }
        catch (OperationCanceledException)
        {
            progress.Finish();
            Console.WriteLine("Імпорт скасовано.");
        }
        catch (DomainException ex)
        {
            progress.Finish();
            Console.WriteLine($"Дані відхилено: {ex.Message}");
        }
        catch (StorageException ex)
        {
            progress.Finish();
            Console.WriteLine($"Технічна помилка: {ex.Message}");
            Console.WriteLine($"   InnerException: {ex.InnerException?.GetType().Name}");
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"Файл не знайдено: {path}");
        }
        catch (DirectoryNotFoundException)
        {
            Console.WriteLine($"Файл не знайдено: {path}");
        }
        finally
        {
            Console.CancelKeyPress -= onCancelKey;
            service.Changed -= onChanged;
        }

        Console.WriteLine($"Подій BookAdded: {events}, примірників у сховищі: {service.All().Count}");
    }

    private static Task<int> Import(LendingService service, ICatalogImporter importer, string path, bool stream,
        IProgress<int> progress, CancellationToken ct) =>
        stream
            ? service.ImportStreamAsync(importer, path, ct)
            : service.ImportAsync(importer, path, progress, ct);

    private static string? Value(string[] args, string name)
    {
        var i = Array.IndexOf(args, name);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
    }

    private static int Number(string[] args, string name) =>
        int.TryParse(Value(args, name), out var value) && value > 0 ? value : 0;
}
