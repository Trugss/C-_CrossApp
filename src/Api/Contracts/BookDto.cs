namespace Api.Contracts;

public sealed record BookDto(string Id, string Isbn, string Title, string Author, int Year, int Copies);
