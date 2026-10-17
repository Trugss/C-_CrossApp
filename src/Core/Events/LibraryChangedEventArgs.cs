namespace Core.Events;

public sealed class LibraryChangedEventArgs(
    string copyId,
    LibraryChangeKind kind,
    string title,
    string? readerId = null,
    DateOnly? dueDate = null) : EventArgs
{
    public string CopyId { get; } = copyId;
    public LibraryChangeKind Kind { get; } = kind;
    public string Title { get; } = title;
    public string? ReaderId { get; } = readerId;     // лише для CopyIssued
    public DateOnly? DueDate { get; } = dueDate;     // до якої дати треба повернути
    public DateTimeOffset At { get; } = DateTimeOffset.Now;
}
