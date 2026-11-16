using Core.Abstractions;

namespace Core.Storage;

public static class CatalogExporters
{
    public static readonly IReadOnlyList<string> Supported = ["json", "csv"];

    public static ICatalogExporter Create(string format) =>
        format.ToLowerInvariant() switch
        {
            "json" => new JsonCatalogExporter(),
            "csv" => new CsvCatalogExporter(),
            _ => throw new DomainException($"Невідомий формат експорту '{format}', підтримуються: {string.Join(", ", Supported)}")
        };

    public static string TargetPath(ICatalogExporter exporter, string dataFile) =>
        Path.ChangeExtension(dataFile, exporter.Extension);
}
