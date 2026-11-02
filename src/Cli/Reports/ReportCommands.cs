using Core;
using Core.Domain;
using Core.Dto;
using Core.Reports;
using Core.Services;

public static class ReportCommands
{
    public const string Usage =
        "report-top | report-overdue | report-genres | report-sum | report-low | report-editions\n" +
        "report-readers | report-overdue-details | report-query | report-deferred | report-bench | report-all\n" +
        "прапорці: --n <число> (розмір топу / поріг / кількість записів), --empty (порожнє джерело)";

    private static readonly string[] OptionsWithValue = ["--import", "--n"];

    public static string? ParseCommand(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (OptionsWithValue.Contains(args[i]))
                i++;
            else if (!args[i].StartsWith("--"))
                return args[i];
        }

        return null;
    }

    public static void Run(string command, LendingService service, string[] args)
    {
        bool empty = args.Contains("--empty");
        var today = DateTime.Today;
        IReadOnlyList<BookCopy> copies = empty ? [] : service.All();
        IReadOnlyList<Loan> loans = empty ? [] : SampleData.Loans(today);
        IReadOnlyList<ReaderDto> readers = empty ? [] : SampleData.Readers();

        Console.WriteLine($"Джерело: примірників {copies.Count}, видач {loans.Count}, читачів {readers.Count}, сьогодні {today:yyyy-MM-dd}");
        Console.WriteLine();

        if (command == "report-all")
        {
            foreach (var c in AllReports)
            {
                Console.WriteLine($"> {c}");
                RunOne(c, copies, loans, readers, today, args);
            }
            return;
        }

        RunOne(command, copies, loans, readers, today, args);
    }

    private static readonly string[] AllReports =
    [
        "report-top", "report-overdue", "report-genres", "report-sum", "report-low",
        "report-editions", "report-readers", "report-overdue-details", "report-query", "report-deferred",
    ];

    private static void RunOne(string command, IReadOnlyList<BookCopy> copies, IReadOnlyList<Loan> loans,
        IReadOnlyList<ReaderDto> readers, DateTime today, string[] args)
    {
        switch (command)
        {
            case "report-top":
                Title($"Топ-{Option(args, 5)} читачів за кількістю видач");
                foreach (var r in ReportService.TopReaders(loans, Option(args, 5)))
                    Console.WriteLine($"   {r.ReaderId,-8} видач: {r.Loans,3}   активних: {r.Active,2}");
                break;

            case "report-overdue":
                Title($"Прострочені видачі (термін < {today:yyyy-MM-dd})");
                PrintOverdue(ReportService.Overdue(loans, today));
                break;

            case "report-genres":
                Title("Фонд за жанрами");
                PrintGroups(ReportService.ByGenre(copies));
                break;

            case "report-sum":
                Title("Підсумки фонду");
                var t = ReportService.Totals(copies);
                Console.WriteLine($"   примірників {t.Copies}, видань {t.Editions}, видано {t.Issued}, " +
                                  $"середнє примірників на видання {t.AverageCopiesPerEdition:F2}, макс {t.MaxCopiesPerEdition}");
                break;

            case "report-low":
                Title($"Дефіцит: видання, де доступних примірників < {Option(args, 1)}");
                PrintEditions(ReportService.ScarceEditions(copies, Option(args, 1)));
                break;

            case "report-editions":
                Title($"Топ-{Option(args, 5)} видань за кількістю примірників");
                PrintEditions(ReportService.TopEditions(copies, Option(args, 5)));
                break;

            case "report-readers":
                Title("Активність читачів (GroupJoin читачі × видачі)");
                var activity = ReportService.ReaderActivity(readers, loans, today);
                if (activity.Count == 0)
                    Console.WriteLine("   (порожньо)");
                foreach (var a in activity)
                    Console.WriteLine($"   {a.ReaderId,-8} {a.FullName,-18} видач: {a.Loans,3}   активних: {a.Active,2}   прострочених: {a.Overdue,2}");
                break;

            case "report-overdue-details":
                Title("Прострочені видачі з назвою і читачем (Join видачі × примірники × читачі)");
                var details = ReportService.OverdueDetails(loans, copies, readers, today);
                if (details.Count == 0)
                    Console.WriteLine("   (порожньо)");
                foreach (var d in details)
                    Console.WriteLine($"   {d.LoanId,-7} {d.Title,-22} {d.ReaderName,-18} термін {d.DueOn:yyyy-MM-dd}   +{d.DaysOverdue} дн.");
                break;

            case "report-query":
                Title("Синтаксис запитів vs методний синтаксис (ByGenreQuery vs ByGenre)");
                var queryResult = ReportService.ByGenreQuery(copies);
                PrintGroups(queryResult);
                Console.WriteLine($"   Результати однакові: {queryResult.SequenceEqual(ReportService.ByGenre(copies))}");
                break;

            case "report-deferred":
                Title("Відкладене виконання");
                var demo = DeferredExecutionDemo.Run(copies);
                Console.WriteLine($"   Викликів предиката після оголошення запиту: {demo.PredicateCallsAfterDeclare}");
                Console.WriteLine($"   Доступних до змін: {demo.Before}");
                Console.WriteLine($"   Доступних після source.Add(...): {demo.AfterAdd}");
                Console.WriteLine($"   У знімку ToList() після source.Clear(): {demo.Snapshot}");
                Console.WriteLine($"   Сам запит після source.Clear(): {demo.QueryAfterClear}");
                Console.WriteLine($"   Усього викликів предиката: {demo.TotalPredicateCalls}");
                break;

            case "report-bench":
                int count = Option(args, 100_000);
                Title($"Stopwatch: «доступні за жанром» на {count:N0} згенерованих примірниках (середнє з 20 запусків)");
                foreach (var b in ReportBenchmark.Run(SampleData.Generate(count)))
                    Console.WriteLine($"   {b.Variant,-38} {b.Milliseconds,9:F2} мс   рядків: {b.Rows}");
                break;

            default:
                Console.WriteLine(Usage);
                break;
        }

        Console.WriteLine();
    }

    private static int Option(string[] args, int defaultValue)
    {
        int i = Array.IndexOf(args, "--n");
        return i >= 0 && i + 1 < args.Length && int.TryParse(args[i + 1], out var value) ? value : defaultValue;
    }

    private static void Title(string text) => Console.WriteLine($"== {text} ==");

    private static void PrintGroups(IReadOnlyList<GroupSummary> groups)
    {
        if (groups.Count == 0)
            Console.WriteLine("   (порожньо)");
        foreach (var g in groups)
            Console.WriteLine($"   {g.Key,-16} примірників: {g.Copies,3}   видань: {g.Editions,3}   видано: {g.Issued,3}");
    }

    private static void PrintEditions(IReadOnlyList<EditionLine> lines)
    {
        if (lines.Count == 0)
            Console.WriteLine("   (порожньо)");
        foreach (var e in lines)
            Console.WriteLine($"   {e.Isbn,-19} {e.Title,-22} примірників: {e.Copies,2}   доступно: {e.Available,2}");
    }

    private static void PrintOverdue(IReadOnlyList<OverdueLine> lines)
    {
        if (lines.Count == 0)
            Console.WriteLine("   (порожньо)");
        foreach (var o in lines)
            Console.WriteLine($"   {o.LoanId,-7} {o.CopyId,-8} {o.ReaderId,-7} термін {o.DueOn:yyyy-MM-dd}   +{o.DaysOverdue} дн.");
    }
}
