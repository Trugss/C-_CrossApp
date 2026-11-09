using Core.Domain;

namespace Core.Tests;

public class LoanTests
{
    private static readonly DateTime Day = new(2026, 11, 1);

    [Fact]
    public void Open_IssuesCopy_AndSetsDueDate()
    {
        var copy = TestData.Copy("C-1");

        var loan = Loan.Open("L-1", copy, "R-1", Day, loanDays: 7);

        Assert.True(copy.IsIssued);
        Assert.Equal(Day.AddDays(7), loan.DueOn);
        Assert.Equal(LoanStatus.Active, loan.Status);
    }

    [Fact]
    public void Close_Twice_ThrowsDomainException_AndKeepsReturnDate()
    {
        var copy = TestData.Copy("C-1");
        var loan = Loan.Open("L-1", copy, "R-1", Day);
        loan.Close(copy, Day.AddDays(3));

        Assert.Throws<DomainException>(() => loan.Close(copy, Day.AddDays(5)));

        Assert.Equal(Day.AddDays(3), loan.ReturnedOn);
        Assert.False(copy.IsIssued);
    }

    [Theory]
    [InlineData(LoanStatus.Returned, LoanStatus.Overdue)]
    [InlineData(LoanStatus.Lost, LoanStatus.Returned)]
    [InlineData(LoanStatus.Active, LoanStatus.Active)]
    public void ChangeStatus_ForbiddenTransition_ThrowsDomainException(LoanStatus from, LoanStatus to)
    {
        var loan = Loan.Open("L-1", TestData.Copy("C-1"), "R-1", Day);
        if (from != LoanStatus.Active)
            loan.ChangeStatus(from);

        Assert.Throws<DomainException>(() => loan.ChangeStatus(to));
        Assert.Equal(from, loan.Status);
    }

    [Theory]
    [InlineData(14, false)]
    [InlineData(15, true)]
    public void IsOverdue_StartsTheDayAfterDueDate(int daysLater, bool expected) =>
        Assert.Equal(expected, Loan.Open("L-1", TestData.Copy("C-1"), "R-1", Day).IsOverdue(Day.AddDays(daysLater)));

    [Fact]
    public void LoanService_SixthActiveLoan_ThrowsDomainException_AndLeavesCopyAvailable()
    {
        var existing = Enumerable.Range(1, LoanService.MaxActiveLoansPerReader)
            .Select(i => Loan.Open($"L-{i}", TestData.Copy($"C-{i}"), "R-1", Day))
            .ToList();
        var copy = TestData.Copy("C-6");

        Assert.Throws<DomainException>(() => new LoanService().IssueBook("L-6", copy, "R-1", existing, Day));

        Assert.False(copy.IsIssued);
    }
}
