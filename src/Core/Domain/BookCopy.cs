using Core.Dto;

namespace Core.Domain;

public sealed class BookCopy
{
    public string Id { get; }
    public string Isbn { get; }
    public string Title { get; }
    public bool IsIssued { get; private set; }

    private BookCopy(string id, string isbn, string title, bool isIssued)
    {
        Id = id;
        Isbn = isbn;
        Title = title;
        IsIssued = isIssued;
    }

    public static BookCopy Create(string id, string isbn, bool isIssued = false, string? title = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор примірника не може бути порожнім", nameof(id));

        if (string.IsNullOrWhiteSpace(isbn))
            throw new ArgumentException("ISBN не може бути порожнім", nameof(isbn));

        return new BookCopy(id.Trim(), isbn.Trim(), title?.Trim() ?? string.Empty, isIssued);
    }

    public void Issue()
    {
        if (IsIssued)
            throw new InvalidOperationException($"Примірник '{Id}' вже виданий, повторна видача неможлива");

        IsIssued = true;
    }

    public void Return()
    {
        if (!IsIssued)
            throw new InvalidOperationException($"Примірник '{Id}' зараз не виданий, повернення неможливе");

        IsIssued = false;
    }

    public BookCopyDto ToDto() => new(Id, Isbn, IsIssued, Title);

    public static BookCopy FromDto(BookCopyDto dto) =>
        Create(dto.Id, dto.Isbn, dto.IsIssued, dto.Title);

    public override string ToString() =>
        $"[Примірник {Id}] ISBN: {Isbn}{(Title.Length > 0 ? $", «{Title}»" : "")}, Стан: {(IsIssued ? "Виданий" : "У наявності")}";
}