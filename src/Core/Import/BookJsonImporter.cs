using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class BookJsonImporter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static ImportResult<BookDto> Load(string path)
    {
        try
        {
            string json = File.ReadAllText(path);
            var items = JsonSerializer.Deserialize<List<BookDto>>(json, Options) ?? [];
            return new ImportResult<BookDto>(items, Array.Empty<string>());
        }
        catch (JsonException ex)
        {
            return new ImportResult<BookDto>(
                Array.Empty<BookDto>(),
                new[] { $"Помилка парсингу JSON файлу: {ex.Message}" });
        }
    }
}