using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Api.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;

namespace Api.Tests;

public sealed class BookApiTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory
        .WithWebHostBuilder(b => b.ConfigureLogging(l => l.ClearProviders()))
        .CreateClient();

    private static SaveBookRequest NewRequest(string isbn, string title = "Земля") =>
        new(isbn, title, "Ольга Кобилянська", 2021, 3);

    [Fact]
    public async Task Post_Returns201_AndGetByLocationReturns200()
    {
        var response = await _client.PostAsJsonAsync("/api/books", NewRequest("978-966-03-0001-1"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var created = await response.Content.ReadFromJsonAsync<BookDto>();
        Assert.NotNull(created);
        Assert.Equal($"/api/books/{created.Id}", response.Headers.Location!.OriginalString);

        var get = await _client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);

        var loaded = await get.Content.ReadFromJsonAsync<BookDto>();
        Assert.Equal(created, loaded);
    }

    [Fact]
    public async Task CrudCycle_GetAfterDelete_Returns404()
    {
        var created = await (await _client.PostAsJsonAsync("/api/books", NewRequest("978-966-03-0002-2")))
            .Content.ReadFromJsonAsync<BookDto>();
        var url = $"/api/books/{created!.Id}";

        var put = await _client.PutAsJsonAsync(url, NewRequest("978-966-03-0002-2", "Земля. Повість"));
        Assert.Equal(HttpStatusCode.OK, put.StatusCode);
        Assert.Equal("Земля. Повість", (await put.Content.ReadFromJsonAsync<BookDto>())!.Title);

        var delete = await _client.DeleteAsync(url);
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var get = await _client.GetAsync(url);
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }

    [Fact]
    public async Task Post_InvalidBody_Returns400WithFieldErrors()
    {
        var response = await _client.PostAsJsonAsync("/api/books", new SaveBookRequest("", "X", "", 3000, -1));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Contains("Isbn", problem.Errors.Keys);
        Assert.Contains("Author", problem.Errors.Keys);
        Assert.Contains("Year", problem.Errors.Keys);
        Assert.Contains("Copies", problem.Errors.Keys);
        Assert.DoesNotContain("Title", problem.Errors.Keys);
    }

    [Fact]
    public async Task Put_EmptyTitle_Returns400()
    {
        var response = await _client.PutAsJsonAsync("/api/books/a1b2c3d4",
            new SaveBookRequest("978-966-03-4004-2", "", "Тарас Шевченко", 2019, 4));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Equal(["Назва обов'язкова, до 200 символів."], problem!.Errors["Title"]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("{\"isbn\":\"978-966-03-1111-1\",\"title\":\"A\",\"author\":\"B\",\"year\":2020,\"copies\":\"abc\"}")]
    [InlineData("{ не json")]
    public async Task Post_UnreadableBody_Returns400NotServerError(string body)
    {
        var response = await _client.PostAsync("/api/books", new StringContent(body, Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var text = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Exception", text);
    }

    [Fact]
    public async Task Post_DuplicateIsbn_Returns409()
    {
        var response = await _client.PostAsJsonAsync("/api/books",
            new SaveBookRequest("978-966-03-4004-2", "Кобзар", "Тарас Шевченко", 2020, 1));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(409, problem!.Status);
        Assert.Contains("978-966-03-4004-2", problem.Detail);
    }

    [Fact]
    public async Task GetMissing_Returns404_AndDeleteMissing_Returns404()
    {
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/books/ffffffff")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.DeleteAsync("/api/books/ffffffff")).StatusCode);
    }

    [Fact]
    public async Task UnexpectedException_Returns500WithoutDetails()
    {
        var response = await _client.PostAsync("/dev/fail", null);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var text = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Exception", text);
        Assert.DoesNotContain("внутрішні деталі", text);
        Assert.Contains("\"status\":500", text);
    }

    [Fact]
    public async Task List_FiltersByAuthor_AndPages()
    {
        var page = await _client.GetFromJsonAsync<PagedResponse<BookDto>>("/api/books?author=martin&page=1&pageSize=1");

        Assert.NotNull(page);
        Assert.Equal(2, page.Total);
        Assert.Single(page.Items);
        Assert.All(page.Items, b => Assert.Equal("Robert C. Martin", b.Author));
    }

    [Fact]
    public async Task List_InvalidPaging_Returns400()
    {
        var response = await _client.GetAsync("/api/books?page=0&pageSize=500");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Contains("Page", problem!.Errors.Keys);
        Assert.Contains("PageSize", problem.Errors.Keys);
    }

    [Fact]
    public async Task OpenApiDocument_DescribesAllBookRoutes()
    {
        using var document = JsonDocument.Parse(await _client.GetStringAsync("/openapi/v1.json"));
        var paths = document.RootElement.GetProperty("paths");

        var methods = paths.GetProperty("/api/books").EnumerateObject().Select(p => p.Name)
            .Concat(paths.GetProperty("/api/books/{id}").EnumerateObject().Select(p => p.Name))
            .Where(n => n is "get" or "post" or "put" or "delete")
            .ToList();

        Assert.Equal(5, methods.Count);
    }
}
