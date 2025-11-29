using System.Diagnostics;

namespace Cetus.Api.Test.Shared.Helpers;

public static class WaitUntilHelper
{
    public static async Task WaitUntilAsync(
        Func<Task<bool>> predicate,
        TimeSpan timeout,
        TimeSpan pollInterval)
    {
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed < timeout)
        {
            if (await predicate().ConfigureAwait(false))
            {
                return;
            }

            await Task.Delay(pollInterval).ConfigureAwait(false);
        }

        throw new TimeoutException("Condition not met within timeout.");
    }
}
