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
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cancellation.Cancel();
            };
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
            catch (TaskCanceledException)
            {
            }
            logger.Information("Goodbye World!");
        }
    }

    /*
     
     prerequisites: password-less ssh access; using root user; rsync (Linux for Windows is perfect); ubuntu 16+; 
https://kimsereyblog.blogspot.com/2018/03/install-dotnet-on-ubuntu-with-linux.html
https://dotnet.microsoft.com/download/linux-package-manager/ubuntu18-04/sdk-current
https://florianbrinkmann.com/en/3436/ssh-key-and-the-windows-subsystem-for-linux/
https://pmcgrath.net/running-a-simple-dotnet-core-linux-daemon
https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-apache?view=aspnetcore-2.1&tabs=aspnetcore2x#create-the-service-file
     * 
     */
}