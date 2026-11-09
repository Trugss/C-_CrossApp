namespace Core.Reports;

public static class EnumerableExtensions
{
    public static IEnumerable<T> TopBy<T>(this IEnumerable<T> source, Func<T, int> key, int n, Func<T, string> tieBreaker)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(tieBreaker);
        ArgumentOutOfRangeException.ThrowIfNegative(n);

        return source
            .OrderByDescending(key)
            .ThenBy(tieBreaker, StringComparer.Ordinal)
            .Take(n);
    }
}
