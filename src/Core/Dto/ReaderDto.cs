namespace Core.Dto;

public sealed record ReaderDto(
    string Id,
    string FullName,
    string Email,
    string? Phone = null);