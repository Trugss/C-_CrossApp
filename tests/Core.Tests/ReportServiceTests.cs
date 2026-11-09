using Core.Domain;
using Core.Reports;

namespace Core.Tests;

public class ReportServiceTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 4)]
    public void ScarceEditions_CountsOnlyBelowThreshold(int threshold, int expected) =>
        Assert.Equal(expected, ReportService.ScarceEditions(TestData.Items(), threshold).Count);

    [Fact]
    public void TopEditions_IsDeterministic_TieBrokenByIsbn() =>
        Assert.Equal(["978-BIG", "978-MID", "978-ZED"],
            ReportService.TopEditions(TestData.Items(), 3).Select(e => e.Isbn));

    [Fact]
    public void Totals_CountsCopiesEditionsAndIssued()
    {
        var totals = ReportService.Totals(TestData.Items());

        Assert.Equal(new LibraryTotals(8, 4, 2d, 3, 3), totals);
    }

    [Fact]
    public void Totals_OfEmptySource_ReturnsEmpty_WithoutException() =>
        Assert.Equal(LibraryTotals.Empty, ReportService.Totals([]));

    [Fact]
    public void Overdue_SortsByDaysOverdue_AndSkipsClosedLoans()
    {
        var today = new DateTime(2026, 11, 20);
        var copies = Enumerable.Range(1, 3).Select(i => TestData.Copy($"C-{i}")).ToList();
        var loans = new List<Loan>
        {
            Loan.Open("L-1", copies[0], "R-1", today.AddDays(-16)),
            Loan.Open("L-2", copies[1], "R-2", today.AddDays(-20)),
            Loan.Open("L-3", copies[2], "R-3", today.AddDays(-30)),
        };
        loans[2].Close(copies[2], today.AddDays(-1));

        var overdue = ReportService.Overdue(loans, today);

        Assert.Equal(["L-2", "L-1"], overdue.Select(o => o.LoanId));
        Assert.Equal([6, 2], overdue.Select(o => o.DaysOverdue));
    }
}
