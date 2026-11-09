using System.Runtime.CompilerServices;
using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class JsonCatalogImporter : ICatalogImporter
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public async Task<IReadOnlyList<BookCopy>> LoadAsync(string path, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        await using var stream = OpenRead(path);

        List<BookCopyDto?> dtos;
        try
        {
            dtos = await JsonSerializer.DeserializeAsync<List<BookCopyDto?>>(stream, Options, ct)
                ?? throw new DomainException($"Файл {Path.GetFileName(path)} не містить списку.");
        }
        catch (JsonException ex)
        {
            throw new StorageException($"Файл {Path.GetFileName(path)} не є коректним JSON: {ex.Message}", ex);
        }

        var result = new List<BookCopy>(dtos.Count);
        for (var i = 0; i < dtos.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            result.Add(ToCopy(dtos[i], i + 1));
        }

        return result;
    }

    public async IAsyncEnumerable<BookCopy> StreamAsync(string path, [EnumeratorCancellation] CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        await using var stream = OpenRead(path);
        var records = JsonSerializer.DeserializeAsyncEnumerable<BookCopyDto?>(stream, Options, ct);
        await using var enumerator = records.GetAsyncEnumerator(ct);

        var number = 0;
        while (true)
        {
            BookCopyDto? dto;
            try
            {
                if (!await enumerator.MoveNextAsync())
                    yield break;
                dto = enumerator.Current;
            }
            catch (JsonException ex)
            {
                throw new StorageException($"Файл {Path.GetFileName(path)} не є коректним JSON: {ex.Message}", ex);
            }

            ct.ThrowIfCancellationRequested();
            yield return ToCopy(dto, ++number);
        }
    }

    private static FileStream OpenRead(string path) =>
        new(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);

    private static BookCopy ToCopy(BookCopyDto? dto, int number)
    {
        if (dto is null)
            throw new DomainException($"Запис {number}: порожній запис");

        try
        {
            return BookCopy.FromDto(dto);
        }
        catch (ArgumentException ex)
        {
            throw new DomainException($"Запис {number}: {ex.Message}", ex);
        }
    }
}
