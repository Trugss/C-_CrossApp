using Api.Contracts;
using Api.Validation;
using Core.Abstractions;
using Core.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public static class BookEndpoints
{
    public static void MapBooks(this WebApplication app)
    {
        var group = app.MapGroup("/api/books").WithTags("Books");

        group.MapGet("/", ([AsParameters] BookListQuery query, CatalogService catalog) =>
                TypedResults.Ok(catalog.Find(query.Author, query.Search, query.Page, query.PageSize).ToResponse()))
            .Validate<BookListQuery>()
            .WithName("ListBooks")
            .WithSummary("Список книг з фільтром за автором, пошуком і пагінацією");

        group.MapGet("/{id}", Results<Ok<BookDto>, NotFound> (string id, ICatalogStore store) =>
                store.GetById(id) is { } found
                    ? TypedResults.Ok(found.ToDto())
                    : TypedResults.NotFound())
            .WithName("GetBook")
            .WithSummary("Книга за id");

        group.MapPost("/", Created<BookDto> (SaveBookRequest request, CatalogService catalog) =>
            {
                var created = catalog.Create(request.Isbn, request.Title, request.Author, request.Year, request.Copies);
                return TypedResults.Created($"/api/books/{created.Id}", created.ToDto());
            })
            .Validate<SaveBookRequest>()
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithName("CreateBook")
            .WithSummary("Додати книгу");

        group.MapPut("/{id}", Results<Ok<BookDto>, NotFound> (string id, SaveBookRequest request, CatalogService catalog) =>
                catalog.Replace(id, request.Isbn, request.Title, request.Author, request.Year, request.Copies) is { } replaced
                    ? TypedResults.Ok(replaced.ToDto())
                    : TypedResults.NotFound())
            .Validate<SaveBookRequest>()
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithName("ReplaceBook")
            .WithSummary("Повністю замінити книгу");

        group.MapDelete("/{id}", Results<NoContent, NotFound> (string id, CatalogService catalog) =>
                catalog.Delete(id)
                    ? TypedResults.NoContent()
                    : TypedResults.NotFound())
            .WithName("DeleteBook")
            .WithSummary("Видалити книгу");
    }
}
