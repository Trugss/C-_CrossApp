using System.Text.Json;
using Core.Services;
using Core.Storage;

namespace Core.Tests;

public class ImportTests
{
    private const string ValidJson = """
        [
          { "id": "J-1", "isbn": "978-1", "isIssued": false, "title": "Перша" },
          { "id": "J-2", "isbn": "978-2", "isIssued": false, "title": "Друга" },
          { "id": "J-3", "isbn": "978-3", "isIssued": true, "title": "Третя" }
        ]
        """;

    [Fact]
    public async Task ImportAsync_WhenAlreadyCancelled_Throws()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var service = new LendingService(new InMemoryBookStore());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync(new JsonCatalogImporter(), "any.json", cts.Token));
    }

    [Fact]
    public async Task ImportAsync_AddsOnlyNewCopies_AndRaisesEventPerAddedCopy()
    {
        var store = new InMemoryBookStore([TestData.Copy("C-1")]);
        var service = new LendingService(store);
        var events = 0;
        service.Changed += (_, _) => events++;
        var importer = new FakeCatalogImporter([TestData.Copy("C-1"), TestData.Copy("C-2"), TestData.Copy("C-3")]);

        var added = await service.ImportAsync(importer, "fake.json");

        Assert.Equal(2, added);
        Assert.Equal(2, events);
        Assert.Equal(3, store.List().Count);
    }

    [Fact]
    public async Task ImportAsync_ReportsProgressUpTo100()
    {
        var service = new LendingService(new InMemoryBookStore());
        var importer = new FakeCatalogImporter(Enumerable.Range(1, 4).Select(i => TestData.Copy($"C-{i}")));
        var progress = new RecordingProgress();

        await service.ImportAsync(importer, "fake.json", progress);

        Assert.Equal([25, 50, 75, 100], progress.Values);
    }

    [Fact]
    public async Task LoadAsync_WithInvalidRecord_ThrowsDomainException_WithRecordNumber()
    {
        var path = TestData.WriteTempJson("""
            [
              { "id": "J-1", "isbn": "978-1", "isIssued": false },
              { "id": "J-2", "isbn": "", "isIssued": false }
            ]
            """);
        try
        {
            var ex = await Assert.ThrowsAsync<DomainException>(() => new JsonCatalogImporter().LoadAsync(path));

            Assert.StartsWith("Запис 2:", ex.Message);
            Assert.IsType<ArgumentException>(ex.InnerException);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task LoadAsync_WithMalformedJson_ThrowsStorageException_KeepingJsonException()
    {
        var path = TestData.WriteTempJson("""[ { "id": "J-1", "isbn": "978-1", "isIssued": ні } ]""");
        try
        {
            var ex = await Assert.ThrowsAsync<StorageException>(() => new JsonCatalogImporter().LoadAsync(path));

            Assert.IsAssignableFrom<JsonException>(ex.InnerException);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task StreamAsync_ReadsRecordsOneByOne_AndStopsWhenCancelled()
    {
        var path = TestData.WriteTempJson(ValidJson);
        try
        {
            using var cts = new CancellationTokenSource();
            var received = new List<string>();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            {
                await foreach (var copy in new JsonCatalogImporter().StreamAsync(path, cts.Token))
                {
                    received.Add(copy.Id);
                    await cts.CancelAsync();
                }
            });

            Assert.Equal(["J-1"], received);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ImportStreamAsync_AddsEveryRecord()
    {
        var store = new InMemoryBookStore();
        var service = new LendingService(store);
        var importer = new FakeCatalogImporter([TestData.Copy("C-1"), TestData.Copy("C-2")]);

        var added = await service.ImportStreamAsync(importer, "fake.json");

        Assert.Equal(2, added);
        Assert.Equal(2, store.List().Count);
    }

    [Fact]
    public async Task RunAsync_WhenTimeoutFires_ThrowsTimeoutException_AndStoreIsUnchanged()
    {
        var store = new InMemoryBookStore();
        var service = new LendingService(store);
        var slow = new FakeCatalogImporter([TestData.Copy("C-1")], TimeSpan.FromSeconds(10));
        using var user = new CancellationTokenSource();

        var ex = await Assert.ThrowsAsync<TimeoutException>(() =>
            TimeoutRunner.RunAsync(t => service.ImportAsync(slow, "fake.json", t), TimeSpan.FromMilliseconds(50), user.Token));

        Assert.IsAssignableFrom<OperationCanceledException>(ex.InnerException);
        Assert.False(user.IsCancellationRequested);
        Assert.Empty(store.List());
    }

    [Fact]
    public async Task RunAsync_WhenUserCancels_ThrowsOperationCanceled_NotTimeout()
    {
        var service = new LendingService(new InMemoryBookStore());
        var slow = new FakeCatalogImporter([TestData.Copy("C-1")], TimeSpan.FromSeconds(10));
        using var user = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            TimeoutRunner.RunAsync(t => service.ImportAsync(slow, "fake.json", t), TimeSpan.FromSeconds(30), user.Token));

        Assert.True(user.IsCancellationRequested);
    }
}
