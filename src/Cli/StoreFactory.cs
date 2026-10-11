using Core;
using Core.Abstractions;
using Core.Storage;

public static class StoreFactory
{
    public const string DefaultFileName = "library.json";

    public static IBookStore Create(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        IBookStore store = args.Contains("--file")
            ? new FileBookStore(DataPath())
            : new InMemoryBookStore(SampleData.BookCopies());

        return args.Contains("--cache")
            ? new CachingBookStore(store)
            : store;
    }

    public static string DataPath() =>
        Path.Combine(AppContext.BaseDirectory, "data", DefaultFileName);
}
