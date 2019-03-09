using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Transactions;
using NUnit.Framework;

namespace Gaev.Blog.Examples
{
    [NonParallelizable]
    public class PartiallyCommittedTransactionTests
    {
        private const string ConnectionString = "server=localhost;database=tempdb;UID=sa;PWD=sa123";

        [Test]
        public async Task Reproduction1()
        {
            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await ExecuteSql("INSERT Logs VALUES(1)");
                await ExecuteSql("INSERT Logs VALUES(2)");

                var iteration = 0;
                await RetryIfDeadlock(async () =>
                {
                    iteration++;
                    if (iteration == 1)
                        await SimulateDeadlock();
                    else
                        await ExecuteSql("INSERT Logs VALUES(3)");
                });

                await ExecuteSql("INSERT Logs VALUES(4)");
                await ExecuteSql("INSERT Logs VALUES(5)");

                transaction.Complete();
            }
        }

        [Test]
        public async Task Reproduction2()
        {
            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await ExecuteSql("INSERT Logs VALUES(1)");
                await ExecuteSql("INSERT Logs VALUES(2)");
                try
                {
                    await SimulateDeadlock();
                }
                catch (SqlException)
                {
                }

                await ExecuteSql("INSERT Logs VALUES(3)");
                await ExecuteSql("INSERT Logs VALUES(4)");

                transaction.Complete();
            }
        }

        [Test]
        public async Task Reproduction3()
        {
            // https://stackoverflow.com/a/5623877/1400547
            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await ExecuteSql("INSERT Logs VALUES(1)");
                await ExecuteSql("INSERT Logs VALUES(2)");
                try
                {
                    await ExecuteSql("INSERT Logs VALUES('three')");
                }
                catch (SqlException)
                {
                }

                await ExecuteSql("INSERT Logs VALUES(3)");
                await ExecuteSql("INSERT Logs VALUES(4)");

                transaction.Complete();
            }
        }

        [Test]
        public async Task Fix()
        {
            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await ExecuteSql("INSERT Logs VALUES(1)");
                await ExecuteSql("INSERT Logs VALUES(2)");

                var iteration = 0;
                using (new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
                    await RetryIfDeadlock(async () =>
                    {
                        iteration++;
                        if (iteration == 1)
                            await SimulateDeadlock();
                        else
                            await ExecuteSql("INSERT Logs VALUES(3)");
                    });

                await ExecuteSql("INSERT Logs VALUES(4)");
                await ExecuteSql("INSERT Logs VALUES(5)");

                transaction.Complete();
            }
        }

        private static async Task RetryIfDeadlock(Func<Task> act)
        {
            // https://stackoverflow.com/a/13159533/1400547
            var retryCount = 0;
            while (retryCount < 3)
                try
                {
                    await act();
                    break;
                }
                catch (SqlException e)
                {
                    if (e.Number == 1205)
                        retryCount++;
                    else
                        throw;
                }
        }

        private static async Task SimulateDeadlock()
        {
            // https://stackoverflow.com/a/39299800/1400547
            await ExecuteSql(
                "IF EXISTS (SELECT * FROM sys.types WHERE name = 'IntIntSet') DROP TYPE [dbo].[IntIntSet]");
            await ExecuteSql("CREATE TYPE dbo.IntIntSet AS TABLE(Value0 Int NOT NULL,Value1 Int NOT NULL)");
            await ExecuteSql("DECLARE @myPK dbo.IntIntSet;");
        }

        [SetUp]
        public async Task CreateTable() => await ExecuteSql(@"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Logs' AND xtype='U')
            CREATE TABLE Logs(Id INT PRIMARY KEY NOT NULL)
            DELETE FROM Logs");

        private static async Task ExecuteSql(string sql)
        {
            using (var con = new SqlConnection(ConnectionString))
            {
                await con.OpenAsync();
                var cmd = con.CreateCommand();
                cmd.CommandText = sql;
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}