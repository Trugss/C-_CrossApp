using Core.Abstractions;
using Core.Domain;

namespace Core.Services;

public sealed class LendingService(IBookStore store)
{
    private readonly IBookStore _store = store ?? throw new ArgumentNullException(nameof(store));

    public BookCopy AddBook(string isbn, string title) =>
        AddBook($"BC-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}", isbn, title);

    public BookCopy AddBook(string id, string isbn, string title)
    {
        var copy = BookCopy.Create(id, isbn, title: title);
        _store.Add(copy);       
        return copy;
    }

    public BookCopy IssueCopy(string id)
    {
        var copy = GetRequired(id);
        copy.Issue();
        _store.Update(copy);
        return copy;
    }

    public BookCopy ReturnCopy(string id)
    {
        var copy = GetRequired(id);
        copy.Return();
        _store.Update(copy);
        return copy;
    }

    public IReadOnlyList<BookCopy> All() => _store.List();

    public BookCopy? Find(string id) => _store.GetById(id);

    public IReadOnlyList<BookCopy> Search(Func<BookCopy, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _store.List().Where(predicate).ToList();
    }

    private BookCopy GetRequired(string id) =>
        _store.GetById(id)
            ?? throw new InvalidOperationException($"Примірника з id='{id}' немає у сховищі");
}
