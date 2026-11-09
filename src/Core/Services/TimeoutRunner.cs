namespace Core.Services;

public static class TimeoutRunner
{
    public static async Task<T> RunAsync<T>(
        Func<CancellationToken, Task<T>> operation, TimeSpan timeout, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeout, TimeSpan.Zero);

        using var timeoutCts = new CancellationTokenSource(timeout);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

        try
        {
            return await operation(linked.Token);
        }
        catch (OperationCanceledException ex) when (timeoutCts.IsCancellationRequested && !ct.IsCancellationRequested)
        {
            throw new TimeoutException($"Операція не завершилася за {timeout.TotalMilliseconds:0} мс", ex);
        }
    }
}
