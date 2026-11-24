using Core.Domain;

namespace Core.Abstractions;

public interface ICatalogStore
{
    IReadOnlyList<Book> List();
    Book? GetById(string id);
    void Add(Book book);
    bool Update(Book book);
    bool Remove(string id);
}
