using Core.Domain;
using Core.Services;

namespace Api.Contracts;

public static class BookMapping
{
    public static BookDto ToDto(this Book book) =>
        new(book.Id, book.Isbn, book.Title, book.Author, book.Year, book.Copies);

    public static PagedResponse<BookDto> ToResponse(this CatalogPage page) =>
        new(page.Items.Select(b => b.ToDto()).ToList(), page.Total, page.Page, page.PageSize);
}
