using Core.Domain;
using Core.Dto;

namespace Core;

public static class SampleData
{
    public static IReadOnlyList<BookCopy> BookCopies() =>
    [
        BookCopy.Create("BC-001", "978-966-03-4004-2", title: "Кобзар", genre: "Поезія"),
        BookCopy.Create("BC-002", "978-966-03-4004-2", title: "Кобзар", genre: "Поезія"),
        BookCopy.Create("BC-003", "978-617-585-007-1", title: "Тигролови", isIssued: true, genre: "Проза"),
        BookCopy.Create("BC-004", "978-966-10-1288-3", title: "Лісова пісня", genre: "Драма"),
        BookCopy.Create("BC-005", "978-966-14-8912-2", title: "Інститутка", genre: "Проза"),
        BookCopy.Create("BC-006", "978-966-14-8912-2", title: "Інститутка", isIssued: true, genre: "Проза"),
        BookCopy.Create("BC-007", "978-617-12-1452-6", title: "Захар Беркут", genre: "Проза"),
        BookCopy.Create("BC-008", "978-966-03-7120-6", title: "Місто", genre: "Проза"),
        BookCopy.Create("BC-009", "978-966-03-7120-6", title: "Місто", genre: "Проза"),
        BookCopy.Create("BC-010", "978-617-679-341-4", title: "Тіні забутих предків", isIssued: true, genre: "Проза"),
        BookCopy.Create("BC-011", "978-0-13-468599-1", title: "Clean Architecture", genre: "Архітектура ПЗ"),
        BookCopy.Create("BC-012", "978-0-32-112521-7", title: "Domain-Driven Design", isIssued: true, genre: "Архітектура ПЗ"),
        BookCopy.Create("BC-013", "978-1-61729-453-6", title: "C# in Depth", genre: "Програмування"),
        BookCopy.Create("BC-014", "978-1-61729-453-6", title: "C# in Depth", genre: "Програмування"),
        BookCopy.Create("BC-015", "978-0-13-235088-4", title: "Clean Code", genre: "Програмування"),
        BookCopy.Create("BC-016", "978-0-20-163361-0", title: "Design Patterns", isIssued: true, genre: "Архітектура ПЗ"),
        BookCopy.Create("BC-017", "978-1-49205-208-6", title: "Programming C# 10", genre: "Програмування"),
        BookCopy.Create("BC-018", "978-966-03-5601-2", title: "Маруся Чурай", genre: "Поезія"),
        BookCopy.Create("BC-019", "978-966-03-5601-2", title: "Маруся Чурай", genre: "Поезія"),
        BookCopy.Create("BC-020", "978-966-10-1288-3", title: "Лісова пісня", genre: "Драма"),
    ];

    public static IReadOnlyList<ReaderDto> Readers() =>
    [
        new("R-001", "Олена Коваленко", "olena.kovalenko@example.com"),
        new("R-002", "Андрій Мельник", "andrii.melnyk@example.com"),
        new("R-003", "Ірина Бондар", "iryna.bondar@example.com"),
        new("R-004", "Тарас Шевчук", "taras.shevchuk@example.com"),
        new("R-005", "Марія Ткаченко", "mariia.tkachenko@example.com"),
        new("R-006", "Богдан Лисенко", "bohdan.lysenko@example.com"),
    ];

    public static IReadOnlyList<Loan> Loans(DateTime today)
    {
        var d = today.Date;

        Loan Active(string id, string copyId, string readerId, int issuedDaysAgo, int loanDays = Loan.DefaultLoanDays) =>
            Loan.FromDto(new LoanDto(id, copyId, readerId, d.AddDays(-issuedDaysAgo), null, d.AddDays(-issuedDaysAgo + loanDays)));

        Loan Returned(string id, string copyId, string readerId, int issuedDaysAgo, int returnedDaysAgo) =>
            Loan.FromDto(new LoanDto(id, copyId, readerId, d.AddDays(-issuedDaysAgo), d.AddDays(-returnedDaysAgo)));

        return
        [
            Active("L-001", "BC-003", "R-001", 30),
            Active("L-002", "BC-010", "R-002", 20),
            Active("L-003", "BC-016", "R-003", 40, loanDays: 21),
            Active("L-004", "BC-012", "R-001", 5),
            Active("L-005", "BC-006", "R-004", 3),
            Returned("L-006", "BC-001", "R-001", 90, 80),
            Returned("L-007", "BC-013", "R-001", 70, 60),
            Returned("L-008", "BC-011", "R-001", 60, 50),
            Returned("L-009", "BC-013", "R-002", 50, 45),
            Returned("L-010", "BC-015", "R-002", 45, 30),
            Returned("L-011", "BC-004", "R-002", 35, 25),
            Returned("L-012", "BC-007", "R-003", 80, 70),
            Returned("L-013", "BC-017", "R-003", 28, 20),
            Returned("L-014", "BC-008", "R-003", 25, 12),
            Returned("L-015", "BC-005", "R-004", 60, 55),
            Returned("L-016", "BC-018", "R-005", 40, 33),
            Returned("L-017", "BC-011", "R-005", 15, 2),
        ];
    }

    private static readonly string[] Genres = ["Поезія", "Проза", "Драма", "Програмування", "Архітектура ПЗ", "Історія", "Фантастика"];

    public static IReadOnlyList<BookCopy> Generate(int count, int seed = 42)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        var random = new Random(seed);
        int editions = Math.Max(1, count / 4);
        var copies = new List<BookCopy>(count);

        for (int i = 0; i < count; i++)
        {
            int edition = random.Next(editions);
            copies.Add(BookCopy.Create(
                $"GEN-{i:D6}",
                $"978-0-00-{edition:D6}-0",
                isIssued: random.Next(3) == 0,
                title: $"Видання {edition}",
                genre: Genres[edition % Genres.Length]));
        }

        return copies;
    }
}
