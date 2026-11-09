using Core.Domain;

namespace Core.Tests;

internal static class TestData
{
    public static IReadOnlyList<BookCopy> Items() =>
    [
        BookCopy.Create("T-01", "978-BIG", title: "Велика", genre: "Проза"),
        BookCopy.Create("T-02", "978-BIG", title: "Велика", genre: "Проза"),
        BookCopy.Create("T-03", "978-BIG", isIssued: true, title: "Велика", genre: "Проза"),
        BookCopy.Create("T-04", "978-ZED", isIssued: true, title: "Остання", genre: "Драма"),
        BookCopy.Create("T-05", "978-ZED", isIssued: true, title: "Остання", genre: "Драма"),
        BookCopy.Create("T-06", "978-MID", title: "Середня", genre: "Поезія"),
        BookCopy.Create("T-07", "978-MID", title: "Середня", genre: "Поезія"),
        BookCopy.Create("T-08", "978-ONE", title: "Єдина", genre: "Поезія"),
    ];

    public static BookCopy Copy(string id, bool isIssued = false) =>
        BookCopy.Create(id, "978-TEST", isIssued, title: $"Книга {id}");

    public static string WriteTempJson(string json)
    {
        var path = Path.Combine(Path.GetTempPath(), $"crossapp-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, json);
        return path;
    }
}
