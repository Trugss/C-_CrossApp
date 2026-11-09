using System.Runtime.CompilerServices;
using Core.Abstractions;
using Core.Domain;

public sealed class SlowCatalogImporter(ICatalogImporter inner, TimeSpan delayPerRecord) : ICatalogImporter
{
    private readonly ICatalogImporter _inner = inner ?? throw new ArgumentNullException(nameof(inner));

    public async Task<IReadOnlyList<BookCopy>> LoadAsync(string path, CancellationToken ct = default)
    {
        var items = await _inner.LoadAsync(path, ct);
        foreach (var _ in items)
            await Task.Delay(delayPerRecord, ct);
        return items;
    }

    public async IAsyncEnumerable<BookCopy> StreamAsync(string path, [EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var item in _inner.StreamAsync(path, ct).WithCancellation(ct))
        {
            await Task.Delay(delayPerRecord, ct);
            yield return item;
        }
    }
}
