using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class FileBookStore(string path) : IBookStore
{
    private readonly Dictionary<string, BookCopy> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _path = Path.GetFullPath(path);
    private bool _loaded;

    public string FilePath => _path;

    private void EnsureLoaded()
    {
        if (_loaded) return;

        if (!File.Exists(_path))
        {
            _loaded = true;
            return;
        }

        List<BookCopyDto?> dtos;
        try
        {
            dtos = JsonSerializer.Deserialize<List<BookCopyDto?>>(File.ReadAllText(_path), CatalogJson.Options) ?? [];
        }
        catch (JsonException ex)
        {
            throw new StorageException($"Файл даних {Path.GetFileName(_path)} пошкоджено: {ex.Message}", ex);
        }

        _cache.Clear();
        for (var i = 0; i < dtos.Count; i++)
        {
            var copy = ToCopy(dtos[i], i + 1);
            _cache[copy.Id] = copy;
        }

        _loaded = true;
    }

    private void Flush()
    {
        var dtos = _cache.Values.Select(c => c.ToDto()).ToList();
        AtomicFile.WriteAllText(_path, JsonSerializer.Serialize(dtos, CatalogJson.Options));
    }

    private BookCopy ToCopy(BookCopyDto? dto, int number)
    {
        if (dto is null)
            throw new DomainException($"Файл даних {Path.GetFileName(_path)}, запис {number}: порожній запис");

        try
        {
            return BookCopy.FromDto(dto);
        }
        catch (ArgumentException ex)
        {
            throw new DomainException($"Файл даних {Path.GetFileName(_path)}, запис {number}: {ex.Message}", ex);
        }
    }

    public IReadOnlyList<BookCopy> List()
    {
        EnsureLoaded();
        return _cache.Values.ToList();
    }

    public BookCopy? GetById(string id)
    {
        EnsureLoaded();
        return _cache.GetValueOrDefault(id);
    }

    public void Add(BookCopy item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureLoaded();

        if (_cache.ContainsKey(item.Id))
            throw new DomainException($"Примірник з id='{item.Id}' уже існує у сховищі");

        _cache.Add(item.Id, item);
        Flush();
    }

    public void Update(BookCopy item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureLoaded();
        _cache[item.Id] = item;
        Flush();
    }

    public bool Remove(string id)
    {
        EnsureLoaded();
        if (!_cache.Remove(id)) return false;
        Flush();
        return true;
    }
}
