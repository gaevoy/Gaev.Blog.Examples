using System;
using System.Threading;
using System.Threading.Tasks;

namespace Gaev.Blog.AzureServiceBusTaskScheduler
{
    class Program
    {
        const string ConnectionString = "...";

        static async Task Main(string[] args)
        {
            var cancellation = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cancellation.Cancel();
            };
            var handler = new HealthyDeveloperHandler();
            var queue = new SchedulerQueue(ConnectionString);
            await queue.Subscribe(handler, cancellation.Token);
        }
    }
}