using Core.Domain;

namespace Core.Domain;

public sealed class LoanService
{
    public const int MaxActiveLoansPerReader = 5;

    public Loan IssueBook(
        string loanId, 
        BookCopy copy, 
        string readerId, 
        IReadOnlyList<Loan> existingLoans, 
        DateTime issuedOn)
    {
        ArgumentNullException.ThrowIfNull(copy, nameof(copy));
        ArgumentNullException.ThrowIfNull(existingLoans, nameof(existingLoans));

        var activeLoansCount = existingLoans.Count(l => l.ReaderId == readerId && !l.IsClosed);

        if (activeLoansCount >= MaxActiveLoansPerReader)
        {
            throw new InvalidOperationException(
                $"Читач '{readerId}' перевищив ліміт активних видач: дозволено максимум {MaxActiveLoansPerReader}, зараз відкрито {activeLoansCount}");
        }

        return Loan.Open(loanId, copy, readerId, issuedOn);
    }
}