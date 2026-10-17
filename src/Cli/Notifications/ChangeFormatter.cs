using Core.Events;

public static class ChangeFormatter
{
    public static string Format(LibraryChangedEventArgs e)
    {
        var details = e.Kind switch
        {
            LibraryChangeKind.BookAdded    => $"«{e.Title}» додано до фонду",
            LibraryChangeKind.CopyIssued   => $"«{e.Title}» видано читачу {e.ReaderId} до дати {e.DueDate:dd.MM.yyyy}",
            LibraryChangeKind.CopyReturned => $"«{e.Title}» повернено до фонду",
            _ => e.Kind.ToString()
        };

        return $"{e.At:HH:mm:ss} {e.Kind,-12} {e.CopyId} {details}";
    }
}
