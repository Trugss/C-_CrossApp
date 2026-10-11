using Core.Domain;

namespace Core;

public static class SampleData
{
    public static IReadOnlyList<BookCopy> BookCopies() =>
    [
        BookCopy.Create("BC-001", "978-966-03-4004-2", title: "Кобзар"),
        BookCopy.Create("BC-002", "978-966-03-4004-2", title: "Кобзар"),
        BookCopy.Create("BC-003", "978-617-585-007-1", title: "Тигролови", isIssued: true),
        BookCopy.Create("BC-004", "978-966-10-1288-3", title: "Лісова пісня"),
        BookCopy.Create("BC-005", "978-966-14-8912-2", title: "Інститутка"),
        BookCopy.Create("BC-006", "978-966-14-8912-2", title: "Інститутка", isIssued: true),
        BookCopy.Create("BC-007", "978-617-12-1452-6", title: "Захар Беркут"),
        BookCopy.Create("BC-008", "978-966-03-7120-6", title: "Місто"),
        BookCopy.Create("BC-009", "978-966-03-7120-6", title: "Місто"),
        BookCopy.Create("BC-010", "978-617-679-341-4", title: "Тіні забутих предків", isIssued: true),
        BookCopy.Create("BC-011", "978-0-13-468599-1", title: "Clean Architecture"),
        BookCopy.Create("BC-012", "978-0-32-112521-7", title: "Domain-Driven Design", isIssued: true),
        BookCopy.Create("BC-013", "978-1-61729-453-6", title: "C# in Depth"),
        BookCopy.Create("BC-014", "978-1-61729-453-6", title: "C# in Depth"),
        BookCopy.Create("BC-015", "978-0-13-235088-4", title: "Clean Code"),
        BookCopy.Create("BC-016", "978-0-20-163361-0", title: "Design Patterns", isIssued: true),
        BookCopy.Create("BC-017", "978-1-49205-208-6", title: "Programming C# 10"),
        BookCopy.Create("BC-018", "978-966-03-5601-2", title: "Маруся Чурай"),
    ];
}
