using System;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Serilog.Core;

namespace Gaev.Blog.Examples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var cancellation = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) => { e.Cancel = true; cancellation.Cancel(); };
            using (var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("HelloSshDeploy.log")
                .CreateLogger())
                await RunApplication(logger, cancellation.Token);
        }

        static async Task RunApplication(Logger logger, CancellationToken cancellation)
        {
            logger.Information("Hello World!");
            try
            {
                await Task.Delay(Timeout.Infinite, cancellation);
            }
            catch (TaskCanceledException) { }
            logger.Information("Goodbye World!");
        }
    }
}