using Core.Abstractions;
using Core.Events;

public sealed class ConsoleSink(string prefix = "[sink]") : INotificationSink
{
    public void Notify(LibraryChangedEventArgs change) =>
        Console.WriteLine($"   {prefix} {ChangeFormatter.Format(change)}");
}
