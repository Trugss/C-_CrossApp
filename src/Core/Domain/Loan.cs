using Core.Dto;

namespace Core.Domain;

public sealed class Loan
{
    public string Id { get; }
    public string BookCopyId { get; }
    public string ReaderId { get; }
    public DateTime IssuedOn { get; }
    public DateTime? ReturnedOn { get; private set; }
    public LoanStatus Status { get; private set; }

    public bool IsClosed => ReturnedOn.HasValue || Status == LoanStatus.Returned || Status == LoanStatus.Lost;

    private Loan(string id, string bookCopyId, string readerId, DateTime issuedOn, DateTime? returnedOn, LoanStatus status)
    {
        Id = id;
        BookCopyId = bookCopyId;
        ReaderId = readerId;
        IssuedOn = issuedOn;
        ReturnedOn = returnedOn;
        Status = status;
    }

    public static Loan Open(string id, BookCopy copy, string readerId, DateTime issuedOn)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор видачі не може бути порожнім", nameof(id));

        ArgumentNullException.ThrowIfNull(copy, nameof(copy));

        if (string.IsNullOrWhiteSpace(readerId))
            throw new ArgumentException("Ідентифікатор читача не може бути порожнім", nameof(readerId));

        copy.Issue();

        return new Loan(id.Trim(), copy.Id, readerId.Trim(), issuedOn, null, LoanStatus.Active);
    }

    public void Close(BookCopy copy, DateTime returnedOn)
    {
        ArgumentNullException.ThrowIfNull(copy, nameof(copy));

        if (copy.Id != BookCopyId)
            throw new ArgumentException($"Невідповідність примірника: очікується '{BookCopyId}', отримано '{copy.Id}'", nameof(copy));

        if (IsClosed)
            throw new InvalidOperationException($"Видача '{Id}' уже закрита зі статусом {Status}");

        if (returnedOn < IssuedOn)
            throw new ArgumentOutOfRangeException(
                nameof(returnedOn),
                returnedOn,
                $"Дата повернення ({returnedOn:yyyy-MM-dd}) не може передувати даті видачі ({IssuedOn:yyyy-MM-dd})");

        copy.Return();
        ReturnedOn = returnedOn;
        Status = LoanStatus.Returned;
    }

    public void ChangeStatus(LoanStatus newStatus)
    {
        Status = (Status, newStatus) switch
        {
            (LoanStatus.Active, LoanStatus.Returned) => LoanStatus.Returned,
            (LoanStatus.Active, LoanStatus.Overdue)  => LoanStatus.Overdue,
            (LoanStatus.Active, LoanStatus.Lost)     => LoanStatus.Lost,

            (LoanStatus.Overdue, LoanStatus.Returned) => LoanStatus.Returned,
            (LoanStatus.Overdue, LoanStatus.Lost)     => LoanStatus.Lost,

            (var current, var target) when current == target =>
                throw new InvalidOperationException($"Видача '{Id}' уже має статус {current}"),

            (var current, var target) =>
                throw new InvalidOperationException($"Неприпустимий перехід стану видачі '{Id}': з {current} у {target}")
        };
    }

    public LoanDto ToDto() => new(Id, BookCopyId, ReaderId, IssuedOn, ReturnedOn);

    public static Loan FromDto(LoanDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Id))
            throw new ArgumentException("Ідентифікатор видачі обов'язковий", nameof(dto));

        if (string.IsNullOrWhiteSpace(dto.BookCopyId))
            throw new ArgumentException("Ідентифікатор книги обов'язковий", nameof(dto));

        if (string.IsNullOrWhiteSpace(dto.ReaderId))
            throw new ArgumentException("Ідентифікатор читача обов'язковий", nameof(dto));

        if (dto.ReturnedOn.HasValue && dto.ReturnedOn.Value < dto.IssuedOn)
            throw new ArgumentOutOfRangeException(
                nameof(dto),
                dto.ReturnedOn.Value,
                $"Дата повернення ({dto.ReturnedOn:yyyy-MM-dd}) не може бути раніше дати видачі ({dto.IssuedOn:yyyy-MM-dd})");

        var initialStatus = dto.ReturnedOn.HasValue ? LoanStatus.Returned : LoanStatus.Active;

        return new Loan(dto.Id.Trim(), dto.BookCopyId.Trim(), dto.ReaderId.Trim(), dto.IssuedOn, dto.ReturnedOn, initialStatus);
    }

    public override string ToString() =>
        $"[Видача {Id}] Примірник: {BookCopyId} -> Читач: {ReaderId} | Видано: {IssuedOn:yyyy-MM-dd} | Статус: {Status}{(ReturnedOn.HasValue ? $" (Повернено {ReturnedOn:yyyy-MM-dd})" : "")}";
}