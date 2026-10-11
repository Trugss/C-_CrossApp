using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Services;
using Core.Storage;

Console.OutputEncoding = System.Text.Encoding.UTF8;

bool useFile = args.Contains("--file");
bool useCache = args.Contains("--cache");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "library.json");

IBookStore store;
if (args.Contains("--factory"))
{
    store = StoreFactory.Create(args);               // 3
}
else
{
    store = useFile
        ? new FileBookStore(dataPath)
        : new InMemoryBookStore(SampleData.BookCopies());

    if (useCache)
        store = new CachingBookStore(store);         // 1
}

var service = new LendingService(store);

Console.WriteLine($"Сховище: {(store is CachingBookStore caching ? $"{nameof(CachingBookStore)} -> {caching.InnerName}" : store.GetType().Name)}");
if (useFile)
    Console.WriteLine($"Файл даних: {dataPath}");

Console.WriteLine();
Console.WriteLine("Сценарій 1: додати, змінити, показати список, знайти за id");

var created = service.AddBook("978-966-03-9999-1", "Кайдашева сім'я");
Console.WriteLine($"Додано:  {created}");

service.IssueCopy(created.Id);
Console.WriteLine($"Видано:  {service.Find(created.Id)}");

Console.WriteLine();
Console.WriteLine($"Усі примірники ({service.All().Count}):");
foreach (var c in service.All())
    PrintRow(c);

Console.WriteLine();
var found = service.Find(created.Id);
Console.WriteLine(found is null
    ? $"Пошук за id '{created.Id}': не знайдено"
    : $"Пошук за id '{created.Id}': {found}");
Console.WriteLine($"Пошук за id 'BC-404': {service.Find("BC-404")?.ToString() ?? "не знайдено"}");

Console.WriteLine();
Console.WriteLine("Сценарій 2: помилки з людським повідомленням");

TryDo("Видача неіснуючого примірника", () => service.IssueCopy("BC-404"));
TryDo("Повернення неіснуючого примірника", () => service.ReturnCopy("BC-404"));
TryDo("Додавання примірника з уже наявним id", () => service.AddBook(created.Id, "978-966-03-0000-0", "Дубль"));
TryDo("Повторна видача вже виданого примірника", () => service.IssueCopy(created.Id));

service.ReturnCopy(created.Id);
Console.WriteLine($"Повернено: {service.Find(created.Id)}");

Console.WriteLine();
Console.WriteLine("Сценарій 3: Додаткові завдання");

Console.WriteLine("-> Пошук з Func<BookCopy, bool> (Додаткове завдання 2):");
var available = service.Search(c => !c.IsIssued);
Console.WriteLine($"   Доступних примірників: {available.Count}");
var csharpBooks = service.Search(c => c.Title.Contains("C#", StringComparison.OrdinalIgnoreCase));
Console.WriteLine($"   Книжок про C#: {csharpBooks.Count}");
foreach (var c in csharpBooks)
    PrintRow(c);

if (store is CachingBookStore cache)
{
    Console.WriteLine();
    Console.WriteLine("-> Декоратор CachingBookStore (Додаткове завдання 1):");
    service.All();
    service.All();
    service.Find(created.Id);
    service.Find(created.Id);
    Console.WriteLine($"   Влучань у кеш: {cache.Hits}, звернень до {cache.InnerName}: {cache.Misses}");
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
