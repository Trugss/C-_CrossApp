using System.Globalization;
using System.Text;
using Core.Dto;

namespace Core.Import;

public static class MixedCsvImporter
{
    private const char Separator = ';';

    public static LibraryBatchResult Load(string path)
    {
        var books = new List<BookDto>();
        var readers = new List<ReaderDto>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path, Encoding.UTF8);

        for (int i = 0; i < lines.Length; i++)
        {
            int num = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            if (num == 1 && line.StartsWith("type", StringComparison.OrdinalIgnoreCase))
                continue;

            switch (ParseLine(line))
            {
                case BookOk b:
                    books.Add(b.Value);
                    break;
                case ReaderOk r:
                    readers.Add(r.Value);
                    break;
                case ParseFailed f:
                    errors.Add($"рядок {num}: {f.Reason}");
                    break;
            }
        }

        return new LibraryBatchResult(books, readers, errors);
    }

    private static Outcome ParseLine(string line)
    {
        string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);

        return parts switch
        {
            { Length: < 4 } => new ParseFailed($"очікую щонайменше 4 колонки, отримано {parts.Length}"),

            ["B", var id, var isbn, var title, var yearStr, ..] when !int.TryParse(yearStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int y) || y < 1450 || y > DateTime.Now.Year =>
                new ParseFailed($"некоректний рік видання книги: '{yearStr}'"),

            ["B", var id, var isbn, var title, var yearStr] =>
                new BookOk(new BookDto(id, isbn, title, int.Parse(yearStr, CultureInfo.InvariantCulture))),

            ["B", var id, var isbn, var title, var yearStr, var author] =>
                new BookOk(new BookDto(id, isbn, title, int.Parse(yearStr, CultureInfo.InvariantCulture), string.IsNullOrWhiteSpace(author) ? null : author)),

            ["R", "", _, _, ..] or ["R", _, "", _, ..] =>
                new ParseFailed("ID або ПІБ читача не можуть бути порожніми"),

            ["R", var id, var name, var email] =>
                new ReaderOk(new ReaderDto(id, name, email)),

            ["R", var id, var name, var email, var phone] =>
                new ReaderOk(new ReaderDto(id, name, email, string.IsNullOrWhiteSpace(phone) ? null : phone)),

            [var unknownType, ..] when unknownType is not ("B" or "R") =>
                new ParseFailed($"невідомий тип запису: '{unknownType}' (очікується B або R)"),

            _ => new ParseFailed($"некоректна кількість колонок: {parts.Length}")
        };
    }

    private abstract record Outcome;
    private sealed record BookOk(BookDto Value) : Outcome;
    private sealed record ReaderOk(ReaderDto Value) : Outcome;
    private sealed record ParseFailed(string Reason) : Outcome;
}