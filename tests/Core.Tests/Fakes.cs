using System.Runtime.CompilerServices;
using Core.Abstractions;
using Core.Domain;

namespace Core.Tests;

internal sealed class FakeCatalogImporter(IEnumerable<BookCopy> items, TimeSpan delay = default) : ICatalogImporter
{
    private readonly IReadOnlyList<BookCopy> _items = items.ToList();

    public async Task<IReadOnlyList<BookCopy>> LoadAsync(string path, CancellationToken ct = default)
    {
        if (delay > TimeSpan.Zero)
            await Task.Delay(delay, ct);
        ct.ThrowIfCancellationRequested();
        return _items;
    }

    public async IAsyncEnumerable<BookCopy> StreamAsync(string path, [EnumeratorCancellation] CancellationToken ct = default)
    {
        foreach (var item in _items)
        {
            ct.ThrowIfCancellationRequested();
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, ct);
            yield return item;
        }
    }
}

internal sealed class RecordingProgress : IProgress<int>
{
    public List<int> Values { get; } = [];

    public void Report(int value) => Values.Add(value);
}
