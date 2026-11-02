namespace Core.Reports;

public sealed record OverdueDetail(string LoanId, string Title, string ReaderName, DateOnly DueOn, int DaysOverdue);
