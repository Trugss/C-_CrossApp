using Core.Domain;

namespace Core.Reports;

// Коли виконується запит: рядок з Where лише будує об'єкт-запит (лічильник викликів предиката = 0).
// Предикат починає викликатися тільки в Count()/ToList(), і щоразу заново проходить поточний вміст source.
// Тому другий Count() після source.Add(...) бачить новий примірник і дає інше число.
// ToList() один раз виконує запит і копіює результат, тож source.Clear() знімок уже не змінює.
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
