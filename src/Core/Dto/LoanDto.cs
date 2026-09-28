namespace Core.Dto;

public sealed record LoanDto(
    string Id,
    string BookCopyId,
    string ReaderId,
    DateTime IssuedOn,
    DateTime? ReturnedOn
);