using Core.Domain;

namespace Core.Services;

public sealed record CatalogPage(IReadOnlyList<Book> Items, int Total, int Page, int PageSize);
