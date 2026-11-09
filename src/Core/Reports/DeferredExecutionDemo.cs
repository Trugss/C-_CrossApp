using Core.Domain;

namespace Core.Reports;

public static class DeferredExecutionDemo
{
    public static DeferredDemoResult Run(IEnumerable<BookCopy> copies)
    {
        var source = copies.ToList();
        int predicateCalls = 0;

        var query = source.Where(c =>
        {
            predicateCalls++;
            return !c.IsIssued;
        });
        int callsAfterDeclare = predicateCalls;

        int before = query.Count();

        source.Add(BookCopy.Create("BC-TMP", "978-0-00-000000-0", title: "Тимчасовий примірник", genre: "Проза"));
        int afterAdd = query.Count();

        var snapshot = query.ToList();
        source.Clear();

        return new DeferredDemoResult(callsAfterDeclare, before, afterAdd, snapshot.Count, query.Count(), predicateCalls);
    }
}
