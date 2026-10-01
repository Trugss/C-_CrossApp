using Core.Domain;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("Сценарій 1: Успіх");

var copy = BookCopy.Create("BC-101", "978-966-03-8000-0");
Console.WriteLine("Створено: " + copy);

var issuedDate = new DateTime(2026, 9, 20);
var loan = Loan.Open("LN-001", copy, "R-501", issuedDate);
Console.WriteLine("Відкрито видачу: " + loan);
Console.WriteLine("Стан примірника після видачі: " + copy);

var returnDate = new DateTime(2026, 9, 28);
loan.Close(copy, returnDate);
Console.WriteLine("Закрито видачу: " + loan);
Console.WriteLine("Стан примірника після повернення: " + copy);

Console.WriteLine();
Console.WriteLine("Сценарій 2: Порушення інваріантів");

TryDo("Порожній ISBN примірника", () => BookCopy.Create("BC-102", "   "));

TryDo("Порожній читач у видачі", () => Loan.Open("LN-002", copy, "", DateTime.Now));

copy.Issue();
TryDo("Повторна видача зайнятого примірника", () => copy.Issue());
TryDo("Відкриття Loan на вже виданий примірник", () => Loan.Open("LN-003", copy, "R-502", DateTime.Now));

copy.Return();
TryDo("Повернення примірника, який не видавався", () => copy.Return());

var activeLoan = Loan.Open("LN-004", copy, "R-503", new DateTime(2026, 9, 25));
TryDo("Дата повернення раніше за дату видачі", () => activeLoan.Close(copy, new DateTime(2026, 9, 10)));

activeLoan.Close(copy, new DateTime(2026, 9, 27));
TryDo("Повторне закриття закритої видачі", () => activeLoan.Close(copy, new DateTime(2026, 9, 28)));

Console.WriteLine();
Console.WriteLine("Сценарій 3: Додаткові завдання");

Console.WriteLine("-> Перевірка доменного сервісу ліміту 5 видач:");
var loanService = new LoanService();
var allLoans = new List<Loan>();
var readerId = "R-777";

for (int i = 1; i <= 5; i++)
{
    var book = BookCopy.Create($"BC-BONUS-{i}", $"978-966-03-{i:0000}-0");
    var l = loanService.IssueBook($"LN-BONUS-{i}", book, readerId, allLoans, DateTime.Now);
    allLoans.Add(l);
}
Console.WriteLine($"   Створено {allLoans.Count} активних видач для читача {readerId}");

var extraBook = BookCopy.Create("BC-BONUS-6", "978-966-03-0006-0");
TryDo("Спроба взяти 6-ту книгу при ліміті 5", () =>
{
    loanService.IssueBook("LN-BONUS-6", extraBook, readerId, allLoans, DateTime.Now);
});

Console.WriteLine();
Console.WriteLine("-> Перевірка статусів LoanStatus та switch-переходів:");
var statusBook = BookCopy.Create("BC-STATUS-1", "978-966-03-8888-0");
var statusLoan = Loan.Open("LN-STATUS-1", statusBook, "R-999", DateTime.Now);

statusLoan.ChangeStatus(LoanStatus.Overdue);
Console.WriteLine($"   Поточний статус: {statusLoan.Status}");

statusLoan.ChangeStatus(LoanStatus.Returned);
Console.WriteLine($"   Поточний статус після повернення: {statusLoan.Status}");

TryDo("Неприпустимий перехід: з Returned назад в Active", () =>
{
    statusLoan.ChangeStatus(LoanStatus.Active);
});

Console.WriteLine();
Console.WriteLine("-> Перевірка пакетної конвертації з ImportResult (Додаткове завдання 1):");

var sampleDtos = new List<Core.Dto.BookCopyDto>
{
    new Core.Dto.BookCopyDto("BC-IMPORT-1", "978-966-03-1234-5", false),
    new Core.Dto.BookCopyDto("BC-IMPORT-2", "   ", false)
};
var sampleImportResult = new Core.Dto.ImportResult<Core.Dto.BookCopyDto>(
    sampleDtos, 
    new List<string> { "Помилка формату CSV у рядку 42" }
);

var (validEntities, conversionErrors) = DomainBatchConverter.ToEntities(sampleImportResult);

Console.WriteLine($"   Успішно створено сутностей: {validEntities.Count}");
Console.WriteLine($"   Усього помилок (імпорт + інваріанти): {conversionErrors.Count}");
foreach (var err in conversionErrors)
{
    Console.WriteLine($"   [ПОМИЛКА ВХІДНИХ ДАНИХ]: {err}");
}

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"[ПОМИЛКА] {title}: виняток НЕ спрацював — інваріант пропущено!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ПЕРЕВІРЕНО] {title}: {ex.GetType().Name} -> {ex.Message}");
    }
}