using System.Text.Json;
using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class JsonCatalogExporter : ICatalogExporter
{
    public string Format => "json";
    public string Extension => ".json";

    public int Export(IEnumerable<BookCopy> items, string path)
    {
        ArgumentNullException.ThrowIfNull(items);
        var dtos = items.Select(c => c.ToDto()).ToList();
        AtomicFile.WriteAllText(path, JsonSerializer.Serialize(dtos, CatalogJson.Options));
        return dtos.Count;
    }
}
