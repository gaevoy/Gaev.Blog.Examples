using System.Threading.Tasks;
using StackExchange.Profiling;
using StackExchange.Profiling.Internal;

namespace Gaev.Blog.Examples
{
    public class ProfilerGetter : IAsyncProfilerProvider
    {
        public ProfilerGetter(MiniProfiler profiler)
        {
            CurrentProfiler = profiler;
        }

        public MiniProfiler Start(string profilerName, MiniProfilerBaseOptions options) => CurrentProfiler;

        public void Stopped(MiniProfiler profiler, bool discardResults)
        {
        }

        public Task StoppedAsync(MiniProfiler profiler, bool discardResults) => Task.CompletedTask;

        public MiniProfiler CurrentProfiler { get; }
    }
}