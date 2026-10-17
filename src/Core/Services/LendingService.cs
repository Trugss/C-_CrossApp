using Core.Abstractions;
using Core.Domain;
using Core.Events;

namespace Core.Services;

public sealed class LendingService(IBookStore store, IEnumerable<INotificationSink>? sinks = null)
{
    public const int DefaultLoanDays = 14;

    private readonly IBookStore _store = store ?? throw new ArgumentNullException(nameof(store));
    private readonly IReadOnlyList<INotificationSink> _sinks = sinks?.ToList() ?? [];

    public event EventHandler<LibraryChangedEventArgs>? Changed;

    private void OnChanged(BookCopy copy, LibraryChangeKind kind, string? readerId = null, DateOnly? dueDate = null)
    {
        var change = new LibraryChangedEventArgs(copy.Id, kind, copy.Title, readerId, dueDate);
        Changed?.Invoke(this, change);

        foreach (var sink in _sinks)
            sink.Notify(change);
    }

    public BookCopy AddBook(string isbn, string title) =>
        AddBook($"BC-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}", isbn, title);

    public BookCopy AddBook(string id, string isbn, string title)
    {
        var copy = BookCopy.Create(id, isbn, title: title);
        _store.Add(copy);
        OnChanged(copy, LibraryChangeKind.BookAdded);
        return copy;
    }

    public BookCopy IssueCopy(string id, string readerId, int loanDays = DefaultLoanDays)
    {
        if (string.IsNullOrWhiteSpace(readerId))
            throw new ArgumentException("Ідентифікатор читача не може бути порожнім", nameof(readerId));

        if (loanDays <= 0)
            throw new ArgumentOutOfRangeException(nameof(loanDays), $"Строк видачі має бути додатним числом днів, отримано {loanDays}");

        var copy = GetRequired(id);
        copy.Issue();
        _store.Update(copy);

        var dueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(loanDays));
        OnChanged(copy, LibraryChangeKind.CopyIssued, readerId.Trim(), dueDate);
        return copy;
    }

    public BookCopy ReturnCopy(string id)
    {
        var copy = GetRequired(id);
        copy.Return();
        _store.Update(copy);
        OnChanged(copy, LibraryChangeKind.CopyReturned);
        return copy;
    }

    public IReadOnlyList<BookCopy> All() => _store.List();

    public BookCopy? Find(string id) => _store.GetById(id);

    public IReadOnlyList<BookCopy> Search(Func<BookCopy, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _store.List().Where(predicate).ToList();
    }

    private BookCopy GetRequired(string id) =>
        _store.GetById(id)
            ?? throw new InvalidOperationException($"Примірника з id='{id}' немає у сховищі");
}
