namespace Core;

public sealed class StorageException : Exception
{
    public StorageException(string message, Exception inner) : base(message, inner) { }
}
