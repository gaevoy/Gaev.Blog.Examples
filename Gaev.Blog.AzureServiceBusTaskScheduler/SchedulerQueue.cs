using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;

namespace Gaev.Blog.AzureServiceBusTaskScheduler
{
    public interface IHandler<TMessage> where TMessage : class, IScheduledMessage
    {
        TMessage GetNext(TMessage previous);
        Task Process(TMessage message);
    }

    public interface IScheduledMessage
    {
        DateTime At { get; set; }
    }

    public class SchedulerQueue
    {
        private readonly string _connectionString;

        public SchedulerQueue(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task Subscribe<TMessage>(IHandler<TMessage> handler, CancellationToken cancellation)
            where TMessage : class, IScheduledMessage
        {
            var queueName = typeof(TMessage).Name;
            var client = new QueueClient(_connectionString, queueName);
            var created = await EnsureQueueCreatedAsync(client.QueueName);
            if (created)
                await client.SendAsync(ToAzureMessage(handler.GetNext(null)));
            client.RegisterMessageHandler(async (azureMessage, _) =>
            {
                var previous = FromAzureMessage<TMessage>(azureMessage);
                await handler.Process(previous);
                await client.SendAsync(ToAzureMessage(handler.GetNext(previous)));
            }, new MessageHandlerOptions(exceptionReceivedHandler: _ => Task.CompletedTask));

            await WaitUntilCancellation(cancellation);
            await client.CloseAsync();
        }

        private async Task<bool> EnsureQueueCreatedAsync(string queueName)
        {
            var client = new ManagementClient(_connectionString);
            if (!await client.QueueExistsAsync(queueName))
            {
                await client.CreateQueueAsync(queueName);
                return true;
            }

            return false;
        }

        private static async Task WaitUntilCancellation(CancellationToken cancellation)
        {
            try
            {
                await Task.Delay(Timeout.Infinite, cancellation);
            }
            catch (TaskCanceledException)
            {
            }
        }

        private static Message ToAzureMessage<TMessage>(TMessage message) where TMessage : IScheduledMessage
        {
            return new Message
            {
                Body = JsonSerializer.SerializeToUtf8Bytes(message),
                ScheduledEnqueueTimeUtc = message.At
            };
        }

        private static TMessage FromAzureMessage<TMessage>(Message message)
        {
            return JsonSerializer.Deserialize<TMessage>(message.Body);
        }
    }
}