namespace Core.Reports;

public sealed record ReaderActivity(string ReaderId, string FullName, int Loans, int Active, int Overdue);
