using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Threading;
using NUnit.Framework;
using StackExchange.Profiling.Data;

namespace Gaev.Blog.Examples
{
    [TestFixture(false, "it uses SqlConnection")]
    [TestFixture(true, "it uses ProfiledDbConnection")]
    public class MiniProfilerReproductionTests
    {
        private readonly bool _useMiniProfiler;

        public MiniProfilerReproductionTests(bool useMiniProfiler, string _)
        {
            _useMiniProfiler = useMiniProfiler;
        }

        [Test]
        public void Sync_transaction_should_not_flow()
        {
            // Given
            Transaction transactionBeforeDbCall = null;
            Transaction transactionAfterDbCall = null;
            using (var tran = new TransactionScope())
            {
                transactionBeforeDbCall = Transaction.Current;
                AsyncPump.Run(async () =>
                {
                    // When
                    await MakeDatabaseCall().ConfigureAwait(false);
                    transactionAfterDbCall = Transaction.Current;
                });
                tran.Complete();
            }

            // Then
            Assert.That(transactionBeforeDbCall, Is.Not.Null);
            Assert.That(transactionAfterDbCall, Is.Null);
        }

        [Test]
        public void Async_transaction_should_flow()
        {
            // Given
            Transaction transactionBeforeDbCall = null;
            Transaction transactionAfterDbCall = null;
            using (var tran = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                transactionBeforeDbCall = Transaction.Current;
                AsyncPump.Run(async () =>
                {
                    // When
                    await MakeDatabaseCall().ConfigureAwait(false);
                    transactionAfterDbCall = Transaction.Current;
                });
                tran.Complete();
            }

            // Then
            Assert.That(transactionBeforeDbCall, Is.Not.Null);
            Assert.That(transactionAfterDbCall, Is.Not.Null);
        }

        [Test]
        public void ConfigureAwait_should_be_respected()
        {
            AsyncPump.Run(async () =>
            {
                // Given
                var contextBeforeDbCall = SynchronizationContext.Current;

                // When
                await MakeDatabaseCall().ConfigureAwait(false);
                var contextAfterDbCall = SynchronizationContext.Current;

                // Then
                Assert.That(contextBeforeDbCall, Is.Not.Null);
                Assert.That(contextAfterDbCall, Is.Null);
            });
        }

        private async Task MakeDatabaseCall()
        {
            using (var con = CreateDbConnection())
            {
                await con.OpenAsync();
                var cmd = con.CreateCommand();
                cmd.CommandText = "select 1;";
                await cmd.ExecuteScalarAsync();
            }
        }

        private DbConnection CreateDbConnection()
        {
            var connection = new SqlConnection("server=localhost;database=tempdb;UID=sa;PWD=sa123");
            if (_useMiniProfiler)
                return new ProfiledDbConnection(connection, null);
            return connection;
        }
    }
}