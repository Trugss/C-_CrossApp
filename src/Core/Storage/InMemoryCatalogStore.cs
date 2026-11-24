using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class InMemoryCatalogStore : ICatalogStore
{
    private readonly Dictionary<string, Book> _items = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _gate = new();

    public InMemoryCatalogStore(IEnumerable<Book>? seed = null)
    {
        foreach (var book in seed ?? [])
            Add(book);
    }

    public IReadOnlyList<Book> List()
    {
        lock (_gate)
            return _items.Values.ToList();
    }

    public Book? GetById(string id)
    {
        lock (_gate)
            return _items.GetValueOrDefault(id);
    }

    public void Add(Book book)
    {
        ArgumentNullException.ThrowIfNull(book);

        lock (_gate)
        {
            if (_items.ContainsKey(book.Id))
                throw new DomainException($"Книга з id='{book.Id}' уже існує у каталозі");

            EnsureUniqueIsbn(book);
            _items.Add(book.Id, book);
        }
    }

    public bool Update(Book book)
    {
        ArgumentNullException.ThrowIfNull(book);

        lock (_gate)
        {
            if (!_items.ContainsKey(book.Id))
                return false;

            EnsureUniqueIsbn(book);
            _items[book.Id] = book;
            return true;
        }
    }

    public bool Remove(string id)
    {
        lock (_gate)
            return _items.Remove(id);
    }

    private void EnsureUniqueIsbn(Book book)
    {
        var other = _items.Values.FirstOrDefault(b =>
            !string.Equals(b.Id, book.Id, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(b.Isbn, book.Isbn, StringComparison.OrdinalIgnoreCase));

        if (other is not null)
            throw new DomainException($"Книга з ISBN {book.Isbn} уже є в каталозі (id='{other.Id}')");
    }
}
