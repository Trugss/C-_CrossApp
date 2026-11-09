public sealed class ConsoleProgress : IProgress<int>
{
    private bool _started;

    public void Report(int value)
    {
        if (!_started)
        {
            Console.Write("Прогрес:");
            _started = true;
        }

        Console.Write($" {value}%");
    }

    public void Finish()
    {
        if (_started)
            Console.WriteLine();
        _started = false;
    }
}
