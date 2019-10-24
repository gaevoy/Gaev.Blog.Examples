using System;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using Microsoft.Azure.ServiceBus;

// ReSharper disable PossibleInvalidOperationException

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
            var scheduler = new ServiceBusJobScheduler(ConnectionString);
            await scheduler.Run(
                queueName: "TakeABreak",
                init: GetInitialMessage,
                job: TakeABreak,
                cancellation.Token
            );
        }

        static Message GetInitialMessage()
        {
            return new Message
            {
                ScheduledEnqueueTimeUtc = DateTime.UtcNow
            };
        }

        static async Task<Message> TakeABreak(Message message)
        {
            // Watch out for exceptions! By default, ServiceBus retries 10 times then move the message into dead-letter queue.
            // Watch out for long-running jobs! By default, ServiceBus waits 5 minutes then returns the message back to the queue. 
            Console.WriteLine($"Take a break! It is {message.ScheduledEnqueueTimeUtc.ToLocalTime():t}.");
            await Task.Delay(100);
            return new Message
            {
                ScheduledEnqueueTimeUtc = CronExpression
                    .Parse("*/30 8-18 * * MON-FRI")
                    .GetNextOccurrence(DateTime.UtcNow, TimeZoneInfo.Local)
                    .Value
            };
        }
    }
}