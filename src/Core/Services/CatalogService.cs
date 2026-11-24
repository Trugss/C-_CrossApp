using System.Security.Cryptography;
using Core.Abstractions;
using Core.Domain;

namespace Core.Services;

public sealed class CatalogService(ICatalogStore store)
{
    private readonly ICatalogStore _store = store ?? throw new ArgumentNullException(nameof(store));

    public CatalogPage Find(string? author = null, string? search = null, int page = 1, int pageSize = 20)
    {
        if (page < 1)
            throw new ArgumentOutOfRangeException(nameof(page), "Номер сторінки починається з 1");

        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Розмір сторінки має бути додатним");

        IEnumerable<Book> query = _store.List();

        if (!string.IsNullOrWhiteSpace(author))
            query = query.Where(b => b.HasAuthor(author));

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(b => b.Matches(search));

        var filtered = query
            .OrderBy(b => b.Author, StringComparer.CurrentCulture)
            .ThenBy(b => b.Title, StringComparer.CurrentCulture)
            .ThenBy(b => b.Id, StringComparer.Ordinal)
            .ToList();

        var items = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return new CatalogPage(items, filtered.Count, page, pageSize);
    }

    public Book? Get(string id) => _store.GetById(id);

    public Book Create(string isbn, string title, string author, int year, int copies)
    {
        var book = Book.Create(NewId(), isbn, title, author, year, copies);
        _store.Add(book);
        return book;
    }

    public Book? Replace(string id, string isbn, string title, string author, int year, int copies)
    {
        if (_store.GetById(id) is null)
            return null;

        var replacement = Book.Create(id, isbn, title, author, year, copies);
        return _store.Update(replacement) ? replacement : null;
    }

    public bool Delete(string id) => _store.Remove(id);

    private static string NewId() =>
        Convert.ToHexString(RandomNumberGenerator.GetBytes(4)).ToLowerInvariant();
}
