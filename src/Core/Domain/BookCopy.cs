using Core.Dto;

namespace Core.Domain;

public sealed class BookCopy
{
    public string Id { get; }
    public string Isbn { get; }
    public string Title { get; }
    public string Genre { get; }
    public bool IsIssued { get; private set; }

    private BookCopy(string id, string isbn, string title, string genre, bool isIssued)
    {
        Id = id;
        Isbn = isbn;
        Title = title;
        Genre = genre;
        IsIssued = isIssued;
    }

    public static BookCopy Create(string id, string isbn, bool isIssued = false, string? title = null, string? genre = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор примірника не може бути порожнім", nameof(id));

        if (string.IsNullOrWhiteSpace(isbn))
            throw new ArgumentException("ISBN не може бути порожнім", nameof(isbn));

        return new BookCopy(id.Trim(), isbn.Trim(), title?.Trim() ?? string.Empty, genre?.Trim() ?? string.Empty, isIssued);
    }

    public void Issue()
    {
        if (IsIssued)
            throw new DomainException($"Примірник '{Id}' уже видано, повторна видача неможлива");

        IsIssued = true;
    }

    public void Return()
    {
        if (!IsIssued)
            throw new DomainException($"Примірник '{Id}' зараз не виданий, повернення неможливе");

        IsIssued = false;
    }

    public BookCopyDto ToDto() => new(Id, Isbn, IsIssued, Title, Genre);

    public static BookCopy FromDto(BookCopyDto dto) =>
        Create(dto.Id, dto.Isbn, dto.IsIssued, dto.Title, dto.Genre);

    public override string ToString() =>
        $"[Примірник {Id}] ISBN: {Isbn}{(Title.Length > 0 ? $", «{Title}»" : "")}, Стан: {(IsIssued ? "Виданий" : "У наявності")}";
}