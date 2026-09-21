using System.Globalization;
using Core.Dto;
using Core.Import;

string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

string fileName = Path.GetFileName(path);
string extension = Path.GetExtension(path).ToLowerInvariant();

if (fileName.Equals("mixed.csv", StringComparison.OrdinalIgnoreCase))
{
    LibraryBatchResult batch = MixedCsvImporter.Load(path);
    int total = batch.Books.Count + batch.Readers.Count + batch.Errors.Count;
    int accepted = batch.Books.Count + batch.Readers.Count;
    double errorRate = total > 0 ? (double)batch.Errors.Count / total * 100 : 0;

    Console.WriteLine($"[ЗМІШАНИЙ ІМПОРТ] Завантажено книг: {batch.Books.Count}, читачів: {batch.Readers.Count}");
    Console.WriteLine($"Статистика: Усього: {total} | Прийнято: {accepted} | Пропущено: {batch.Errors.Count} | Помилок: {errorRate:F1}%");

    if (batch.Errors.Count > 0)
    {
        Console.WriteLine("Помилки:");
        foreach (string err in batch.Errors)
            Console.WriteLine($" ! {err}");
    }
    return 0;
}

ImportResult<BookDto> result = extension switch
{
    ".csv" => BookCsvImporter.Load(path),
    ".json" => BookJsonImporter.Load(path),
    _ => new ImportResult<BookDto>(Array.Empty<BookDto>(), new[] { $"Непідтримуваний формат файлу: {extension}" })
};

Console.WriteLine($"Завантажено записів: {result.Items.Count}");
Console.WriteLine(new string('-', 75));

foreach (BookDto book in result.Items.Take(5))
{
    Console.WriteLine($" {book.Id,-6} | {book.Year,4} | {book.Title,-32} | {book.Author ?? "Не вказано"}");
}

if (result.Errors.Count > 0)
{
    Console.WriteLine(new string('-', 75));
    Console.WriteLine($"Пропущено рядків: {result.Errors.Count}");
    foreach (string err in result.Errors)
    {
        Console.WriteLine($" ! {err}");
    }
}

int totalRows = result.Items.Count + result.Errors.Count;
double errorPercent = totalRows > 0 ? (double)result.Errors.Count / totalRows * 100 : 0;
Console.WriteLine(new string('=', 75));
Console.WriteLine($"Статистика: Усього: {totalRows} | Прийнято: {result.Items.Count} | Пропущено: {result.Errors.Count} | Помилок: {errorPercent:F1}%");

return 0;