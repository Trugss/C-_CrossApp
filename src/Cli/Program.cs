using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Events;
using Core.Import;
using Core.Services;
using Core.Storage;

Console.OutputEncoding = System.Text.Encoding.UTF8;

bool useFile = args.Contains("--file");
bool useCache = args.Contains("--cache");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "library.json");

IBookStore store;
if (args.Contains("--factory"))
{
    store = StoreFactory.Create(args);
}
else
{
    store = useFile
        ? new FileBookStore(dataPath)
        : new InMemoryBookStore(SampleData.BookCopies());

    if (useCache)
        store = new CachingBookStore(store);
}

var service = new LendingService(store);

Console.WriteLine($"Сховище: {(store is CachingBookStore caching ? $"{nameof(CachingBookStore)} -> {caching.InnerName}" : store.GetType().Name)}");
if (useFile)
    Console.WriteLine($"Файл даних: {dataPath}");

var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");
Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
File.AppendAllText(logPath, $"--- запуск {DateTime.Now:yyyy-MM-dd HH:mm:ss} ---{Environment.NewLine}");
Console.WriteLine($"Лог подій: {logPath}");

void OnLibraryChanged(object? sender, LibraryChangedEventArgs e)
{
    var line = ChangeFormatter.Format(e);
    Console.WriteLine($"   [подія] {line}");
    try
    {
        File.AppendAllText(logPath, line + Environment.NewLine);
    }
    catch (IOException ex)
    {
        Console.WriteLine($"   [подія] не вдалося записати лог: {ex.Message}");
    }
}

service.Changed += OnLibraryChanged;

int changes = 0;
service.Changed += (_, _) => changes++;

Console.WriteLine();
Console.WriteLine("Сценарій 1: додати, видати, показати список, знайти за id");

EventHandler<LibraryChangedEventArgs> orderProbe =
    (_, _) => Console.WriteLine($"   [підписник 3] обробники 1 і 2 вже відпрацювали, changes = {changes}");
service.Changed += orderProbe;

var created = service.AddBook("978-966-03-9999-1", "Кайдашева сім'я");
Console.WriteLine($"Додано:  {created}");

service.Changed -= orderProbe;

service.IssueCopy(created.Id, "R-001");
Console.WriteLine($"Видано:  {service.Find(created.Id)}");

service.ReturnCopy(created.Id);
Console.WriteLine($"Повернено: {service.Find(created.Id)}");
Console.WriteLine($"Подій отримано: {changes}");

Console.WriteLine();
Console.WriteLine($"Усі примірники ({service.All().Count}):");
foreach (var c in service.All())
    PrintRow(c);

Console.WriteLine();
Console.WriteLine($"Пошук за id '{created.Id}': {service.Find(created.Id)?.ToString() ?? "не знайдено"}");
Console.WriteLine($"Пошук за id 'BC-404': {service.Find("BC-404")?.ToString() ?? "не знайдено"}");

Console.WriteLine();
Console.WriteLine("Сценарій 2: відмова — виняток є, події немає");

int before = changes;
TryDo("Видача неіснуючого примірника", () => service.IssueCopy("BC-404", "R-001"));
TryDo("Повернення неіснуючого примірника", () => service.ReturnCopy("BC-404"));
TryDo("Додавання примірника з уже наявним id", () => service.AddBook(created.Id, "978-966-03-0000-0", "Дубль"));
TryDo("Повернення не виданого примірника", () => service.ReturnCopy(created.Id));
TryDo("Видача з від'ємним строком", () => service.IssueCopy(created.Id, "R-002", -3));
TryDo("Видача без читача", () => service.IssueCopy(created.Id, " "));
Console.WriteLine($"Подій отримано: {changes} (до сценарію було {before})");

Console.WriteLine();
Console.WriteLine("Сценарій 3: відписка -= і тиха операція");

service.IssueCopy(created.Id, "R-002", 7);
TryDo("Повторна видача вже виданого примірника", () => service.IssueCopy(created.Id, "R-003"));
Console.WriteLine($"Подій отримано: {changes}");

service.Changed -= OnLibraryChanged;
service.ReturnCopy(created.Id);
Console.WriteLine($"Після відписки: {service.Find(created.Id)}");
Console.WriteLine($"Подій отримано: {changes}  (+1 лише від лямбди-лічильника)");

Console.WriteLine();
Console.WriteLine($"Останні рядки {Path.GetFileName(logPath)}:");
foreach (var line in File.ReadLines(logPath).TakeLast(4))
    Console.WriteLine($"   {line}");

Console.WriteLine();
Console.WriteLine("Сценарій 4: Func замість події там, де потрібне значення");

var available = service.Search(c => !c.IsIssued);
Console.WriteLine($"   Доступних примірників: {available.Count}");
var csharpBooks = service.Search(c => c.Title.Contains("C#", StringComparison.OrdinalIgnoreCase));
Console.WriteLine($"   Книжок про C#: {csharpBooks.Count}");
foreach (var c in csharpBooks)
    PrintRow(c);

if (store is CachingBookStore cache)
{
    Console.WriteLine();
    Console.WriteLine("-> Декоратор CachingBookStore (ЛР5):");
    service.All();
    service.All();
    service.Find(created.Id);
    service.Find(created.Id);
    Console.WriteLine($"   Влучань у кеш: {cache.Hits}, звернень до {cache.InnerName}: {cache.Misses}");
}

Console.WriteLine();
Console.WriteLine("Сценарій 5: додаткові завдання ЛР6");

Console.WriteLine("-> INotificationSink через конструктор (Додаткове завдання 1):");
var sinkLogPath = Path.Combine(AppContext.BaseDirectory, "logs", "sink.log");
var sinkService = new LendingService(store, [new ConsoleSink(), new FileSink(sinkLogPath)]);
var sinkCopy = sinkService.AddBook("978-617-12-0001-5", "Intermezzo");
sinkService.IssueCopy(sinkCopy.Id, "R-003");
Console.WriteLine($"   Подій отримано лічильником: {changes} (sinkService — інший об'єкт, на нього ніхто не підписаний)");
Console.WriteLine($"   Файл: {sinkLogPath}");

Console.WriteLine();
Console.WriteLine("-> Action<string> для звіту про пропущені рядки (Додаткове завдання 2):");
var importPath = ImportPath(args);
if (File.Exists(importPath))
{
    int skipped = 0;
    var result = BookCsvImporter.Load(importPath, reason =>
    {
        skipped++;
        Console.WriteLine($"   пропущено {reason}");
    });
    Console.WriteLine($"   Імпортовано: {result.Items.Count}, пропущено: {skipped}");
}
else
{
    Console.WriteLine($"   Файл {importPath} не знайдено (запускайте з кореня рішення або передайте --import <шлях>)");
}

static string ImportPath(string[] args)
{
    int i = Array.IndexOf(args, "--import");
    return i >= 0 && i + 1 < args.Length
        ? args[i + 1]
        : Path.Combine("data", "sample.csv");
}

static void PrintRow(BookCopy c) =>
    Console.WriteLine($"   {c.Id,-12} {c.Isbn,-18} {c.Title,-24} {(c.IsIssued ? "виданий" : "у наявності")}");

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"[ПОМИЛКА] {title}: виняток НЕ спрацював!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ПЕРЕВІРЕНО] {title}: {ex.Message}");
    }
}
