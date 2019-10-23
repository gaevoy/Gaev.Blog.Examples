using System;
using System.Threading.Tasks;
using Cronos;

namespace Gaev.Blog.AzureServiceBusTaskScheduler
{
    public class TakeABreak : IScheduledMessage
    {
        public int Index { get; set; }
        public DateTime At { get; set; }
    }

    public class HealthyDeveloperHandler : IHandler<TakeABreak>
    {
        public TakeABreak GetNext(TakeABreak previous)
        {
            return new TakeABreak
            {
                Index = previous?.Index ?? 0 + 1,
                At = CronExpression
                         //.Parse("*/30 8-18 * * MON-FRI")
                         .Parse("*/1 * * * MON-FRI")
                         .GetNextOccurrence(DateTime.UtcNow, TimeZoneInfo.Local) ?? DateTime.MaxValue
            };
        }

        public async Task Process(TakeABreak message)
        {
            Console.WriteLine($"Take a break #{message.Index} it is {message.At.ToLocalTime():t}");
            await Task.Delay(100);
        }
    }
}