using Core.Domain;
using Core.Storage;

namespace Core.Tests;

public sealed class FileBookStoreTests : IDisposable
{
    private readonly TempDirectory _temp = new();

    public void Dispose() => _temp.Dispose();

    [Fact]
    public void Add_ThenNewInstance_ListReturnsSameCopy()
    {
        var path = _temp.File("library.json");
        new FileBookStore(path).Add(BookCopy.Create("C-1", "978-1", true, "Кобзар", "Поезія"));

        var copies = new FileBookStore(path).List();

        var copy = Assert.Single(copies);
        Assert.Equal(("C-1", "978-1", true, "Кобзар", "Поезія"), (copy.Id, copy.Isbn, copy.IsIssued, copy.Title, copy.Genre));
    }

    [Fact]
    public void Flush_WritesCamelCaseAndCyrillicWithoutEscaping_AndLeavesNoTmpFile()
    {
        var path = _temp.File("library.json");
        new FileBookStore(path).Add(BookCopy.Create("C-1", "978-1", false, "Цемент і Кобзар"));

        var json = File.ReadAllText(path);

        Assert.Contains("\"title\": \"Цемент і Кобзар\"", json);
        Assert.DoesNotContain("\\u0426", json);
        Assert.False(File.Exists(path + ".tmp"));
    }

    [Fact]
    public void List_WhenFileMissing_ReturnsEmpty()
    {
        var store = new FileBookStore(_temp.File(Path.Combine("missing", "library.json")));

        Assert.Empty(store.List());
    }

    [Fact]
    public void List_WithMalformedJson_ThrowsStorageException()
    {
        var path = _temp.File("library.json");
        File.WriteAllText(path, "[ { \"id\": \"C-1\", ");

        var ex = Assert.Throws<StorageException>(() => new FileBookStore(path).List());

        Assert.IsType<System.Text.Json.JsonException>(ex.InnerException, exactMatch: false);
    }

    [Fact]
    public void List_WithInvalidRecord_ThrowsDomainExceptionWithRecordNumber()
    {
        var path = _temp.File("library.json");
        File.WriteAllText(path, """[ { "id": "C-1", "isbn": "978-1" }, { "id": "C-2", "isbn": " " } ]""");

        var ex = Assert.Throws<DomainException>(() => new FileBookStore(path).List());

        Assert.Contains("запис 2", ex.Message);
    }
}
