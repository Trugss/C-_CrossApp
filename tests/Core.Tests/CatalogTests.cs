using Core.Domain;
using Core.Services;
using Core.Storage;

namespace Core.Tests;

public class CatalogTests
{
    private static CatalogService NewService() => new(new InMemoryCatalogStore(SampleData.Catalog()));

    [Fact]
    public void Book_Create_RejectsNonPositiveCopies()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Book.Create("id-1", "978-1", "Назва", "Автор", 2020, 0));
    }

    [Fact]
    public void Book_Create_RejectsTooLongTitle()
    {
        Assert.Throws<ArgumentException>(() =>
            Book.Create("id-1", "978-1", new string('a', Book.MaxTitleLength + 1), "Автор", 2020, 1));
    }

    [Fact]
    public void Store_Add_DuplicateIsbn_ThrowsDomainException()
    {
        var store = new InMemoryCatalogStore(SampleData.Catalog());

        var ex = Assert.Throws<DomainException>(() =>
            store.Add(Book.Create("new-1", "978-966-03-4004-2", "Кобзар", "Тарас Шевченко", 2020, 1)));

        Assert.Contains("978-966-03-4004-2", ex.Message);
    }

    [Fact]
    public void Store_Update_MissingId_ReturnsFalse()
    {
        var store = new InMemoryCatalogStore();

        Assert.False(store.Update(Book.Create("nope", "978-1", "Назва", "Автор", 2020, 1)));
    }

    [Fact]
    public async Task Store_ParallelAdds_AreThreadSafe()
    {
        var store = new InMemoryCatalogStore();

        await Task.WhenAll(Enumerable.Range(0, 200).Select(i => Task.Run(() =>
            store.Add(Book.Create($"id-{i}", $"978-{i:D6}", $"Книга {i}", "Автор", 2020, 1)))));

        Assert.Equal(200, store.List().Count);
    }

    [Fact]
    public void Service_CreateReplaceDelete_Cycle()
    {
        var service = NewService();

        var created = service.Create("978-966-03-9999-1", "Земля", "Ольга Кобилянська", 2021, 3);
        Assert.Equal(8, created.Id.Length);
        Assert.Same(created, service.Get(created.Id));

        var replaced = service.Replace(created.Id, "978-966-03-9999-1", "Земля. Повість", "Ольга Кобилянська", 2022, 5);
        Assert.Equal("Земля. Повість", replaced!.Title);
        Assert.Equal(created.Id, replaced.Id);

        Assert.True(service.Delete(created.Id));
        Assert.Null(service.Get(created.Id));
        Assert.Null(service.Replace(created.Id, "978-966-03-9999-1", "X", "Y", 2022, 1));
    }

    [Fact]
    public void Service_Find_FiltersByAuthorAndPages()
    {
        var page = NewService().Find(author: "martin", page: 2, pageSize: 1);

        Assert.Equal(2, page.Total);
        Assert.Equal("Clean Code", Assert.Single(page.Items).Title);
    }

    [Fact]
    public void Service_Find_SearchMatchesTitleAuthorAndIsbn()
    {
        var service = NewService();

        Assert.Equal("Лісова пісня", Assert.Single(service.Find(search: "лісова").Items).Title);
        Assert.Equal("Jon Skeet", Assert.Single(service.Find(search: "453-6").Items).Author);
    }
}
