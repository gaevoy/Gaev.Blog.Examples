using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;

namespace Gaev.Blog.AzureServiceBusTaskScheduler
{
    public class ServiceBusJobScheduler
    {
        private readonly string _connectionString;

        public ServiceBusJobScheduler(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task Run(
            string queueName,
            Func<Message> init,
            Func<Message, Task<Message>> job,
            CancellationToken cancellation
        )
        {
            var queueClient = new QueueClient(_connectionString, queueName);
            var created = await EnsureQueueCreated(queueName);
            if (created)
                await queueClient.SendAsync(init());
            queueClient.RegisterMessageHandler(
                handler: async (message, _) =>
                {
                    var nextMessage = await job(message);
                    if (nextMessage != null)
                        await queueClient.SendAsync(nextMessage);
                },
                exceptionReceivedHandler: _ => Task.CompletedTask
            );
            await Wait(cancellation);
            await queueClient.CloseAsync();
        }

        private async Task<bool> EnsureQueueCreated(string queueName)
        {
            var client = new ManagementClient(_connectionString);
            for (int probe = 1;; probe++)
                try
                {
                    if (!await client.QueueExistsAsync(queueName))
                    {
                        await client.CreateQueueAsync(queueName);
                        return true;
                    }

                    return false;
                }
                catch (ServiceBusException) when (probe < 3)
                {
                    await Task.Delay(100);
                }
        }

        private static async Task Wait(CancellationToken cancellation)
        {
            try
            {
                await Task.Delay(Timeout.Infinite, cancellation);
            }
            catch (TaskCanceledException)
            {
            }
        }
    }
}