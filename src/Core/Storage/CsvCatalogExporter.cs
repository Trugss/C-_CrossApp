using System.Text;
using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class CsvCatalogExporter : ICatalogExporter
{
    public const string Header = "id,isbn,title,genre,isIssued";

    public string Format => "csv";
    public string Extension => ".csv";

    public int Export(IEnumerable<BookCopy> items, string path)
    {
        ArgumentNullException.ThrowIfNull(items);
        var builder = new StringBuilder().AppendLine(Header);
        var count = 0;
        foreach (var c in items)
        {
            builder.AppendLine(string.Join(",", Escape(c.Id), Escape(c.Isbn), Escape(c.Title), Escape(c.Genre),
                c.IsIssued ? "true" : "false"));
            count++;
        }

        AtomicFile.WriteAllText(path, builder.ToString());
        return count;
    }

    public static string Escape(string value) =>
        value.IndexOfAny([',', '"', '\n', '\r']) >= 0
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;
}
