namespace Core.Reports;

public sealed record LibraryTotals(int Copies, int Editions, double AverageCopiesPerEdition, int MaxCopiesPerEdition, int Issued)
{
    public static LibraryTotals Empty { get; } = new(0, 0, 0d, 0, 0);
}
