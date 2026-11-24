namespace Api.Contracts;

public sealed record BookListQuery(string? Author, string? Search, int Page = 1, int PageSize = 20);
