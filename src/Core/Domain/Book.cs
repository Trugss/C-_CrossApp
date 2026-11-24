namespace Core.Domain;

public sealed class Book
{
    public const int MaxTitleLength = 200;
    public const int MaxAuthorLength = 100;
    public const int MinYear = 1450;
    public const int MaxCopies = 1000;

    public string Id { get; }
    public string Isbn { get; }
    public string Title { get; }
    public string Author { get; }
    public int Year { get; }
    public int Copies { get; }

    private Book(string id, string isbn, string title, string author, int year, int copies)
    {
        Id = id;
        Isbn = isbn;
        Title = title;
        Author = author;
        Year = year;
        Copies = copies;
    }

    public static Book Create(string id, string isbn, string title, string author, int year, int copies)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор книги не може бути порожнім", nameof(id));

        if (string.IsNullOrWhiteSpace(isbn))
            throw new ArgumentException("ISBN не може бути порожнім", nameof(isbn));

        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > MaxTitleLength)
            throw new ArgumentException($"Назва обов'язкова, до {MaxTitleLength} символів", nameof(title));

        if (string.IsNullOrWhiteSpace(author) || author.Trim().Length > MaxAuthorLength)
            throw new ArgumentException($"Автор обов'язковий, до {MaxAuthorLength} символів", nameof(author));

        if (year < MinYear || year > DateTime.Today.Year)
            throw new ArgumentOutOfRangeException(nameof(year), $"Рік видання має бути від {MinYear} до {DateTime.Today.Year}");

        if (copies < 1 || copies > MaxCopies)
            throw new ArgumentOutOfRangeException(nameof(copies), $"Кількість примірників має бути від 1 до {MaxCopies}");

        return new Book(id.Trim(), isbn.Trim(), title.Trim(), author.Trim(), year, copies);
    }

    public bool HasAuthor(string author) =>
        Author.Contains(author.Trim(), StringComparison.CurrentCultureIgnoreCase);

    public bool Matches(string text)
    {
        var value = text.Trim();
        return Title.Contains(value, StringComparison.CurrentCultureIgnoreCase)
            || Author.Contains(value, StringComparison.CurrentCultureIgnoreCase)
            || Isbn.Contains(value, StringComparison.OrdinalIgnoreCase);
    }

    public override string ToString() => $"[Книга {Id}] {Author}, «{Title}», {Year}, ISBN {Isbn}, примірників: {Copies}";
}
