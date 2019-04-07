using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using NUnit.Framework;

#pragma warning disable 1998

// ReSharper disable MethodSupportsCancellation
// ReSharper disable AccessToDisposedClosure

#pragma warning disable 4014

namespace Gaev.Blog.Examples
{
    [NonParallelizable]
    public class SqlQueueTests
    {
        private const string ConnectionString = "server=localhost;database=tempdb;UID=sa;PWD=sa123";

        [SetUp]
        public Task CreateQueue() => ExecuteSql(@"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='MyQueue' AND xtype='U')
            CREATE TABLE MyQueue(Id int PRIMARY KEY IDENTITY(1,1),	Payload nvarchar(max))
            DELETE FROM MyQueue");

        [Test]
        public async Task It_should_fetch_different_batches()
        {
            // Given
            for (var i = 1; i <= 100; i++)
                await Publish("Message#" + i);
            var batches = new List<List<Message>>();
            var subscribing = new CancellationTokenSource(5000);

            // When
            var numberOfSubscribers = 10;
            for (var i = 1; i <= numberOfSubscribers; i++)
                Subscribe(async messages =>
                {
                    lock (batches)
                        batches.Add(messages);
                    await Task.Delay(Timeout.Infinite, subscribing.Token);
                }, subscribing.Token);

            // Then
            while (!subscribing.IsCancellationRequested)
                lock (batches)
                    if (batches.Count == numberOfSubscribers)
                        break;
            subscribing.Cancel();
            var duplicates = batches
                .SelectMany(messages => messages)
                .GroupBy(message => message.Payload)
                .Where(group => group.Count() > 1)
                .ToList();
            Assert.That(duplicates, Is.Empty);
        }

        [Test]
        public async Task It_should_not_block_publisher()
        {
            // Given
            for (var i = 1; i <= 10; i++)
                await Publish("Message#" + i);
            var subscribing = new CancellationTokenSource(5000);
            Subscribe(
                messages => Task.Delay(Timeout.Infinite, subscribing.Token),
                subscribing.Token,
                batchSize: 10);

            // When
            var publishTasks = Task.CompletedTask;
            for (var i = 1; i <= 10; i++)
                publishTasks = Task.WhenAll(publishTasks, Publish("Message#" + i));

            // Then
            await Task.Delay(200, subscribing.Token);
            subscribing.Cancel();
            Assert.That(publishTasks.Status, Is.EqualTo(TaskStatus.RanToCompletion));
        }

        [Test]
        public async Task It_should_remove_processed()
        {
            // Given
            for (var i = 1; i <= 10; i++)
                await Publish("Message#" + i);
            var subscribing = new CancellationTokenSource(5000);

            // When
            await Subscribe(async messages =>
            {
                messages.Find(e => e.Payload == "Message#5").IsProcessed = true;
                subscribing.Cancel();
            }, subscribing.Token);

            // Then
            var noneProcessed = (await SelectSql("SELECT * FROM MyQueue")).Select(e => e.Payload);
            Assert.That(noneProcessed, Is.Not.Contain("Message#5"));
        }

        [Test]
        public async Task It_should_allow_internal_transaction()
        {
            // Given
            for (var i = 1; i <= 10; i++)
                await Publish("Message#" + i);
            var subscribing = new CancellationTokenSource(5000);

            // When
            await Subscribe(async messages =>
            {
                foreach (var message in messages)
                    message.IsProcessed = true;
                using (new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
                    try
                    {
                        await SimulateDeadlock();
                    }
                    catch (Exception ex)
                    {
                        var _ = "";
                    }

                subscribing.Cancel();
            }, subscribing.Token);

            // Then
            var noneProcessed = (await SelectSql("SELECT * FROM MyQueue")).Select(e => e.Payload);
            Assert.That(noneProcessed, Is.Empty);
        }

        [Test]
        public async Task It_should_allow_internal_transaction2()
        {
            // Given
            for (var i = 1; i <= 10; i++)
                await Publish("Message#" + i);
            var subscribing = new CancellationTokenSource(5000);

            // When
            await Subscribe(async messages =>
            {
                foreach (var message in messages)
                    message.IsProcessed = true;
                using (new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
                using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    await ExecuteSql("INSERT MyQueue VALUES('oops')");

                subscribing.Cancel();
            }, subscribing.Token);

            // Then
            var noneProcessed = (await SelectSql("SELECT * FROM MyQueue")).Select(e => e.Payload);
            Assert.That(noneProcessed, Is.Empty);
        }

        [Test]
        public async Task It_should_allow_internal_transaction3()
        {
            // Given
            for (var i = 1; i <= 10; i++)
                await Publish("Message#" + i);
            var subscribing = new CancellationTokenSource(5000);

            // When
            await Subscribe(async messages =>
            {
                foreach (var message in messages)
                    message.IsProcessed = true;
                using (new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
                using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await ExecuteSql("INSERT MyQueue VALUES('oops')");
                    transaction.Complete();
                }

                subscribing.Cancel();
            }, subscribing.Token);

            // Then
            var noneProcessed = (await SelectSql("SELECT * FROM MyQueue")).Select(e => e.Payload);
            Assert.That(noneProcessed, Is.All.Contains("oops"));
        }

        [Test]
        public async Task It_should_allow_internal_transaction4()
        {
            // Given
            for (var i = 1; i <= 10; i++)
                await Publish("Message#" + i);
            var subscribing = new CancellationTokenSource(5000);

            // When
            await Subscribe(async messages =>
            {
                foreach (var message in messages)
                    message.IsProcessed = true;
                using (new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
                using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await ExecuteSql("UPDATE MyQueue SET Payload = 'oops'");
                    transaction.Complete();
                }

                subscribing.Cancel();
            }, subscribing.Token);

            // Then
            var noneProcessed = (await SelectSql("SELECT * FROM MyQueue")).Select(e => e.Payload).ToList();
            Assert.That(noneProcessed, Has.Count.EqualTo(10));
            Assert.That(noneProcessed, Is.Not.Contain("oops"));
        }

        [Test]
        public async Task It_should_allow_internal_transaction5()
        {
            // Given
            for (var i = 1; i <= 10; i++)
                await Publish("Message#" + i);
            var subscribing = new CancellationTokenSource(5000);

            // When
            await Subscribe(async messages =>
            {
                foreach (var message in messages)
                    message.IsProcessed = true;
                // reusing current transaction
                // if batchsize = 1 it is OK to reuse current transaction, thinking about rollback logic
                // if batchsize > 1 + rollback???
                await ExecuteSql("UPDATE MyQueue SET Payload = 'oops'");

                subscribing.Cancel();
            }, subscribing.Token);

            // Then
            var noneProcessed = (await SelectSql("SELECT * FROM MyQueue")).Select(e => e.Payload).ToList();
            Assert.That(noneProcessed, Has.Count.EqualTo(10));
            Assert.That(noneProcessed, Is.Not.Contain("oops"));
        }

        private Task Publish(string message)
        {
            // TODO: Fix SQL injection
            return ExecuteSql($"INSERT MyQueue VALUES('{message}')");
        }

        private async Task Subscribe(
            Func<List<Message>, Task> handler,
            CancellationToken cancellation,
            int batchSize = 10,
            int pollingInterval = 100)
        {
            while (!cancellation.IsCancellationRequested)
                try
                {
                    using (var transaction = BeginTransaction(IsolationLevel.ReadCommitted))
                    {
                        var messages =
                            await SelectSql($"SELECT TOP {batchSize} * FROM MyQueue WITH (UPDLOCK, READPAST)");
                        await handler(messages);
                        var processedIds = messages.Where(e => e.IsProcessed).Select(e => e.Id).ToList();
                        if (processedIds.Any())
                            await ExecuteSql($"DELETE FROM MyQueue WHERE Id IN ({string.Join(",", processedIds)})");
                        transaction.Complete();
                    }

                    await Task.Delay(pollingInterval, cancellation);
                }
                catch (TaskCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    // log and retry
                }
        }

        private static TransactionScope BeginTransaction(IsolationLevel isolationLevel) => new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions {IsolationLevel = isolationLevel},
            TransactionScopeAsyncFlowOption.Enabled);

        private static async Task ExecuteSql(string sql)
        {
            using (var con = new SqlConnection(ConnectionString))
            {
                await con.OpenAsync();
                var cmd = con.CreateCommand();
                cmd.CommandTimeout = 1;
                cmd.CommandText = sql;
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private static async Task<List<Message>> SelectSql(string sql)
        {
            using (var con = new SqlConnection(ConnectionString))
            {
                await con.OpenAsync();
                var cmd = con.CreateCommand();
                cmd.CommandTimeout = 1;
                cmd.CommandText = sql;
                var result = new List<Message>();
                using (var dtr = await cmd.ExecuteReaderAsync())
                    while (await dtr.ReadAsync())
                        result.Add(new Message {Id = (int) dtr["Id"], Payload = (string) dtr["Payload"]});
                return result;
            }
        }

        private static async Task SimulateDeadlock()
        {
            // https://stackoverflow.com/a/39299800/1400547
            using (Transaction.Current == null ? new TransactionScope() : null)
            {
                await ExecuteSql(
                    "IF EXISTS (SELECT * FROM sys.types WHERE name = 'IntIntSet') DROP TYPE [dbo].[IntIntSet]");
                await ExecuteSql("CREATE TYPE dbo.IntIntSet AS TABLE(Value0 Int NOT NULL,Value1 Int NOT NULL)");
                await ExecuteSql("DECLARE @myPK dbo.IntIntSet;");
            }
        }
    }

    public class Message
    {
        public int Id { get; set; }
        public string Payload { get; set; }
        public bool IsProcessed { get; set; }
        public override string ToString() => Payload;
    }
}