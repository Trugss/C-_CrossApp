namespace Api.Contracts;

public sealed record SaveBookRequest(string Isbn, string Title, string Author, int Year, int Copies);
