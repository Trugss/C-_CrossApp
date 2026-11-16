using Core.Domain;

namespace Core.Abstractions;

public interface ICatalogExporter
{
    string Format { get; }
    string Extension { get; }
    int Export(IEnumerable<BookCopy> items, string path);
}
