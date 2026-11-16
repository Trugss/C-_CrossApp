using Core.Domain;
using Core.Storage;

namespace Core.Tests;

public sealed class ExporterTests : IDisposable
{
    private readonly TempDirectory _temp = new();

    public void Dispose() => _temp.Dispose();

    private static readonly BookCopy[] Copies =
    [
        BookCopy.Create("C-1", "978-1", false, "Кобзар", "Поезія"),
        BookCopy.Create("C-2", "978-2", true, "Тіні, забутих \"предків\"", "Проза")
    ];

    [Theory]
    [InlineData("json", typeof(JsonCatalogExporter), ".json")]
    [InlineData("CSV", typeof(CsvCatalogExporter), ".csv")]
    public void Create_ReturnsExporterForFormat(string format, Type expected, string extension)
    {
        var exporter = CatalogExporters.Create(format);

        Assert.IsType(expected, exporter);
        Assert.Equal(extension, exporter.Extension);
    }

    [Fact]
    public void Create_UnknownFormat_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => CatalogExporters.Create("xml"));
    }

    [Fact]
    public void JsonExport_CanBeReadBackByFileBookStore()
    {
        var path = _temp.File("library.json");

        var count = new JsonCatalogExporter().Export(Copies, path);

        Assert.Equal(2, count);
        Assert.Equal(["C-1", "C-2"], new FileBookStore(path).List().Select(c => c.Id).Order());
    }

    [Fact]
    public void CsvExport_WritesHeaderAndEscapesCommasAndQuotes()
    {
        var path = CatalogExporters.TargetPath(new CsvCatalogExporter(), _temp.File("library.json"));

        new CsvCatalogExporter().Export(Copies, path);
        var lines = File.ReadAllLines(path);

        Assert.EndsWith(".csv", path);
        Assert.Equal(CsvCatalogExporter.Header, lines[0]);
        Assert.Equal("C-2,978-2,\"Тіні, забутих \"\"предків\"\"\",Проза,true", lines[2]);
    }
}
