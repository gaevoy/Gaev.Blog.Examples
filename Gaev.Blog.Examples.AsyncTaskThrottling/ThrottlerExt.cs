using System;
using System.Threading;
using System.Threading.Tasks;

public static class ThrottlerExt
{
    public static async Task<IDisposable> Throttle(this SemaphoreSlim throttler)
    {
        await throttler.WaitAsync();
        return new Throttler(throttler);
    }

    private class Throttler : IDisposable
    {
        private readonly SemaphoreSlim _throttler;

        public Throttler(SemaphoreSlim throttler) => _throttler = throttler;

        public void Dispose() => _throttler.Release();
    }
}