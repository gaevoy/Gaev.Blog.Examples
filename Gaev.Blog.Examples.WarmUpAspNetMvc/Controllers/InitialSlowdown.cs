using System.Collections.Concurrent;
using System.Threading;

public class InitialSlowdown
{
    private static readonly ConcurrentDictionary<string, bool> IsInitialized =
        new ConcurrentDictionary<string, bool>();

    public static void For(string section)
    {
        IsInitialized.GetOrAdd(section, _ =>
        {
            Thread.Sleep(2000);
            return true;
        });
    }
}