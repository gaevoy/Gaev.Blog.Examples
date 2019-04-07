using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Tests
{
    
    // Topics are needed to show forwarding logic without session and with it
    public class AzureServiceBus : IDisposable
    {
        private readonly MessagingFactory _factory;
        private readonly Dictionary<string, QueueClient> _queueClient;

        public AzureServiceBus(string connectionString)
        {
            _factory = MessagingFactory.CreateFromConnectionString(connectionString);
            _queueClient = new Dictionary<string, QueueClient>
            {
                {"test1", _factory.CreateQueueClient("test1")}
            };
        }

        public Task SendToQueue(string queueName, BrokeredMessage message)
        {
            return _queueClient[queueName].SendAsync(message);
        }

        public async Task SubscribeToQueue(string queueName, Func<BrokeredMessage, Task> onMessage,
            CancellationToken cancellation)
        {
            var receiver = await _factory.CreateMessageReceiverAsync(queueName, ReceiveMode.PeekLock);
            while (!cancellation.IsCancellationRequested)
                try
                {
                    var message = await receiver.ReceiveAsync();
                    if (message != null)
                    {
                        await onMessage(message);
                        // await message.CompleteAsync();
                        // await message.DeadLetterAsync("ProcessingError", "Don't know what to do with this message");
                    }
                }
                catch (MessagingException e)
                {
                    if (!e.IsTransient)
                        throw;
                }

            await receiver.CloseAsync();
        }

        public void Dispose()
        {
            foreach (var client in _queueClient.Values)
                client.Close();
            _factory.Close();
        }
    }
}