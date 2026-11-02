namespace Core.Reports;

public sealed record OverdueLine(string LoanId, string CopyId, string ReaderId, DateOnly DueOn, int DaysOverdue);
