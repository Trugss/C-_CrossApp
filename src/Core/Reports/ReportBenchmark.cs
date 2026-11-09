using System.Diagnostics;
using Core.Domain;

namespace Core.Reports;

public static class ReportBenchmark
{
    public static IReadOnlyList<BenchmarkResult> Run(IReadOnlyList<BookCopy> copies, int repeats = 20)
    {
        ArgumentNullException.ThrowIfNull(copies);
        ArgumentOutOfRangeException.ThrowIfLessThan(repeats, 1);

        return
        [
            Measure("один ланцюжок, ToList у кінці", repeats, () =>
                copies.Where(c => !c.IsIssued)
                    .GroupBy(c => c.Genre)
                    .Select(g => (g.Key, Count: g.Count()))
                    .OrderByDescending(x => x.Count)
                    .ToList().Count),

            Measure("ToList після Where", repeats, () =>
                copies.Where(c => !c.IsIssued)
                    .ToList()
                    .GroupBy(c => c.Genre)
                    .Select(g => (g.Key, Count: g.Count()))
                    .OrderByDescending(x => x.Count)
                    .ToList().Count),

            Measure("ToList після кожного кроку", repeats, () =>
                copies.Where(c => !c.IsIssued).ToList()
                    .GroupBy(c => c.Genre).ToList()
                    .Select(g => (g.Key, Count: g.Count())).ToList()
                    .OrderByDescending(x => x.Count)
                    .ToList().Count),

            Measure("запит без ToList, використано 3 рази", repeats, () =>
            {
                var query = copies.Where(c => !c.IsIssued).GroupBy(c => c.Genre).Select(g => (g.Key, Count: g.Count()));
                int groups = query.Count();
                int largest = query.Max(x => x.Count);
                var rows = query.ToList();
                return groups + largest > 0 ? rows.Count : 0;
            }),

            Measure("ToList один раз, використано 3 рази", repeats, () =>
            {
                var rows = copies.Where(c => !c.IsIssued).GroupBy(c => c.Genre).Select(g => (g.Key, Count: g.Count())).ToList();
                int groups = rows.Count;
                int largest = rows.Max(x => x.Count);
                return groups + largest > 0 ? rows.Count : 0;
            }),
        ];
    }

    private static BenchmarkResult Measure(string variant, int repeats, Func<int> report)
    {
        int rows = 0;
        for (int i = 0; i < 3; i++)
            rows = report();

        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < repeats; i++)
            rows = report();
        stopwatch.Stop();

        return new BenchmarkResult(variant, stopwatch.Elapsed.TotalMilliseconds / repeats, rows);
    }
}
