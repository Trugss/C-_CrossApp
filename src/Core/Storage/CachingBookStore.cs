using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class CachingBookStore(IBookStore inner) : IBookStore
{
    private readonly IBookStore _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    private readonly Dictionary<string, BookCopy?> _byId = new(StringComparer.OrdinalIgnoreCase);
    private IReadOnlyList<BookCopy>? _list;

    public int Hits { get; private set; }
    public int Misses { get; private set; }
    public string InnerName => _inner.GetType().Name;

    public IReadOnlyList<BookCopy> List()
    {
        if (_list is not null)
        {
            Hits++;
            return _list;
        }

        Misses++;
        _list = _inner.List();
        return _list;
    }

    public BookCopy? GetById(string id)
    {
        if (_byId.TryGetValue(id, out var cached))
        {
            Hits++;
            return cached;
        }

        Misses++;
        var copy = _inner.GetById(id);
        _byId[id] = copy;
        return copy;
    }

    public void Add(BookCopy item)
    {
        _inner.Add(item);
        Invalidate(item.Id);
    }

    public void Update(BookCopy item)
    {
        _inner.Update(item);
        Invalidate(item.Id);
    }

    public bool Remove(string id)
    {
        var removed = _inner.Remove(id);
        if (removed) Invalidate(id);
        return removed;
    }

    private void Invalidate(string id)
    {
        _list = null;
        _byId.Remove(id);
    }
}
