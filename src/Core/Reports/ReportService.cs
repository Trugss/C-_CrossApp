using Core.Domain;
using Core.Dto;

namespace Core.Reports;

// Кожен метод приймає IEnumerable і закінчується ToList: запит виконується тут один раз,
// а Cli отримує готовий IReadOnlyList і може перебирати його скільки завгодно.
public static class ReportService
{
    private const string NoGenre = "(без жанру)";

    // 1) Топ-N читачів за кількістю видач: GroupBy + Select у DTO + TopBy (OrderByDescending/ThenBy/Take)
    public static IReadOnlyList<ReaderLoanLine> TopReaders(IEnumerable<Loan> loans, int n) =>
        loans.GroupBy(l => l.ReaderId, StringComparer.Ordinal)
            .Select(g => new ReaderLoanLine(g.Key, g.Count(), g.Count(l => !l.IsClosed)))
            .TopBy(r => r.Loans, n, r => r.ReaderId)
            .ToList();

    // 2) Прострочені видачі на дату today: Where + Select у DTO + OrderByDescending/ThenBy
    public static IReadOnlyList<OverdueLine> Overdue(IEnumerable<Loan> loans, DateTime today) =>
        loans.Where(l => l.IsOverdue(today))
            .Select(l => new OverdueLine(
                l.Id, l.BookCopyId, l.ReaderId,
                DateOnly.FromDateTime(l.DueOn),
                (today.Date - l.DueOn.Date).Days))
            .OrderByDescending(o => o.DaysOverdue)
            .ThenBy(o => o.LoanId, StringComparer.Ordinal)
            .ToList();

    // 3) Групування фонду за жанром: GroupBy + Select (Count, Distinct, Count з умовою) + OrderBy
    public static IReadOnlyList<GroupSummary> ByGenre(IEnumerable<BookCopy> copies) =>
        copies.GroupBy(c => c.Genre, StringComparer.OrdinalIgnoreCase)
            .Select(g => new GroupSummary(
                g.Key.Length == 0 ? NoGenre : g.Key,
                g.Count(),
                g.Select(c => c.Isbn).Distinct().Count(),
                g.Count(c => c.IsIssued)))
            .OrderByDescending(s => s.Copies)
            .ThenBy(s => s.Key, StringComparer.Ordinal)
            .ToList();

    // 4) Підсумки: Count, Sum, Average, Max з обробкою порожнього джерела
    public static LibraryTotals Totals(IEnumerable<BookCopy> copies)
    {
        var list = copies.ToList();
        if (list.Count == 0)
            return LibraryTotals.Empty;

        var copiesPerEdition = list.GroupBy(c => c.Isbn).Select(g => g.Count()).ToList();

        return new LibraryTotals(
            list.Count,
            copiesPerEdition.Count,
            copiesPerEdition.Average(),
            copiesPerEdition.Max(),
            list.Count(c => c.IsIssued));
    }

    // 5) Дефіцит: видання, у яких доступних примірників менше за threshold
    public static IReadOnlyList<EditionLine> ScarceEditions(IEnumerable<BookCopy> copies, int threshold) =>
        Editions(copies)
            .Where(e => e.Available < threshold)
            .OrderBy(e => e.Available)
            .ThenBy(e => e.Isbn, StringComparer.Ordinal)
            .ToList();

    // 6) Топ-N видань за кількістю примірників (другий звіт, що використовує TopBy)
    public static IReadOnlyList<EditionLine> TopEditions(IEnumerable<BookCopy> copies, int n) =>
        Editions(copies)
            .TopBy(e => e.Copies, n, e => e.Isbn)
            .ToList();

    // Додаткове завдання 1а: GroupJoin читачів з видачами (лівий зв'язок: читач без видач теж у звіті)
    public static IReadOnlyList<ReaderActivity> ReaderActivity(
        IEnumerable<ReaderDto> readers, IEnumerable<Loan> loans, DateTime today) =>
        readers.GroupJoin(
                loans,
                r => r.Id,
                l => l.ReaderId,
                (r, readerLoans) => new ReaderActivity(
                    r.Id,
                    r.FullName,
                    readerLoans.Count(),
                    readerLoans.Count(l => !l.IsClosed),
                    readerLoans.Count(l => l.IsOverdue(today))),
                StringComparer.Ordinal)
            .OrderByDescending(a => a.Loans)
            .ThenBy(a => a.ReaderId, StringComparer.Ordinal)
            .ToList();

    // Додаткове завдання 1б: Join видач з примірниками і читачами у зведений DTO
    public static IReadOnlyList<OverdueDetail> OverdueDetails(
        IEnumerable<Loan> loans, IEnumerable<BookCopy> copies, IEnumerable<ReaderDto> readers, DateTime today) =>
        loans.Where(l => l.IsOverdue(today))
            .Join(copies, l => l.BookCopyId, c => c.Id, (l, c) => (Loan: l, Copy: c), StringComparer.Ordinal)
            .Join(readers, x => x.Loan.ReaderId, r => r.Id, (x, r) => new OverdueDetail(
                    x.Loan.Id,
                    x.Copy.Title,
                    r.FullName,
                    DateOnly.FromDateTime(x.Loan.DueOn),
                    (today.Date - x.Loan.DueOn.Date).Days),
                StringComparer.Ordinal)
            .OrderByDescending(d => d.DaysOverdue)
            .ThenBy(d => d.LoanId, StringComparer.Ordinal)
            .ToList();

    // Додаткове завдання 3: той самий звіт, що й ByGenre, у синтаксисі запитів
    public static IReadOnlyList<GroupSummary> ByGenreQuery(IEnumerable<BookCopy> copies) =>
        (from c in copies
         group c by c.Genre into g
         let summary = new GroupSummary(
             g.Key.Length == 0 ? NoGenre : g.Key,
             g.Count(),
             g.Select(c => c.Isbn).Distinct().Count(),
             g.Count(c => c.IsIssued))
         orderby summary.Copies descending, summary.Key
         select summary)
        .ToList();

    private static IEnumerable<EditionLine> Editions(IEnumerable<BookCopy> copies) =>
        copies.GroupBy(c => c.Isbn, StringComparer.Ordinal)
            .Select(g => new EditionLine(g.Key, g.First().Title, g.Count(), g.Count(c => !c.IsIssued)));
}
