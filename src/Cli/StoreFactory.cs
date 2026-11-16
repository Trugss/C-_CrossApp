using Core;
using Core.Abstractions;
using Core.Storage;

public static class StoreFactory
{
    public static IBookStore Create(string[] args, string dataPath)
    {
        ArgumentNullException.ThrowIfNull(args);

        IBookStore store = args.Contains("--file")
            ? new FileBookStore(dataPath)
            : new InMemoryBookStore(SampleData.BookCopies());

        return args.Contains("--cache")
            ? new CachingBookStore(store)
            : store;
    }
}
