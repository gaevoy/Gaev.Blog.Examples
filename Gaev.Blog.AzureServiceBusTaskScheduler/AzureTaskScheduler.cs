using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;

namespace Gaev.Blog.AzureServiceBusTaskScheduler
{
    public class AzureTaskScheduler
    {
        private readonly string _connectionString;

        public AzureTaskScheduler(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task Run(
            string queueName,
            Func<Message, Task<Message>> task,
            CancellationToken cancellation
        )
        {
            var client = new QueueClient(_connectionString, queueName);
            var created = await EnsureQueueCreatedAsync(client.QueueName);
            if (created)
                await client.SendAsync(await task(null));
            client.RegisterMessageHandler(async (message, _) =>
            {
                var next = await task(message);
                if (next != null)
                    await client.SendAsync(next);
            }, new MessageHandlerOptions(OnError));

            try
            {
                await Task.Delay(Timeout.Infinite, cancellation);
            }
            catch (TaskCanceledException)
            {
            }

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

        private static Task OnError(ExceptionReceivedEventArgs e)
        {
            Console.WriteLine(e.Exception);
            return Task.CompletedTask;
        }
    }
}