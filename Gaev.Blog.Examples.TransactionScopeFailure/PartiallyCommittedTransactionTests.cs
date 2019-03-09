using System;
using System.Data.SqlClient;
using System.Transactions;
using NUnit.Framework;

namespace Gaev.Blog.Examples
{
    [NonParallelizable]
    public class PartiallyCommittedTransactionTests
    {
        private const string ConnectionString = "server=localhost;database=tempdb;UID=sa;PWD=sa123";

        [Test]
        public void Reproduction1()
        {
            using (var transaction = new TransactionScope())
            {
                ExecuteSql("INSERT Logs VALUES(1)");

                var iteration = 0;
                RetryIfDeadlock(() =>
                {
                    iteration++;
                    if (iteration == 1)
                        SimulateDeadlock();
                    else
                        ExecuteSql("INSERT Logs VALUES(2)");
                });

                ExecuteSql("INSERT Logs VALUES(3)");

                transaction.Complete();
            }
        }

        [Test]
        public void Reproduction2()
        {
            using (var transaction = new TransactionScope())
            {
                ExecuteSql("INSERT Logs VALUES(1)");
                try
                {
                    SimulateDeadlock();
                }
                catch (SqlException)
                {
                }

                ExecuteSql("INSERT Logs VALUES(2)");
                ExecuteSql("INSERT Logs VALUES(3)");

                transaction.Complete();
            }
        }

        [Test]
        public void Reproduction3()
        {
            // https://stackoverflow.com/a/5623877/1400547
            using (var transaction = new TransactionScope())
            {
                ExecuteSql("INSERT Logs VALUES(1)");
                try
                {
                    ExecuteSql("INSERT Logs VALUES('two')");
                }
                catch (SqlException)
                {
                }

                ExecuteSql("INSERT Logs VALUES(3)");

                transaction.Complete();
            }
        }

        [Test]
        public void Fix1()
        {
            var iteration = 0;
            RetryIfDeadlock(() =>
            {
                iteration++;
                using (var transaction = new TransactionScope())
                {
                    ExecuteSql("INSERT Logs VALUES(1)");
                    if (iteration == 1)
                        SimulateDeadlock();
                    else
                        ExecuteSql("INSERT Logs VALUES(2)");
                    ExecuteSql("INSERT Logs VALUES(3)");

                    transaction.Complete();
                }
            });
        }

        [Test]
        public void Fix2()
        {
            using (var transaction = new TransactionScope())
            {
                ExecuteSql("INSERT Logs VALUES(1)");

                var iteration = 0;
                using (new TransactionScope(TransactionScopeOption.Suppress))
                    RetryIfDeadlock(() =>
                    {
                        iteration++;
                        if (iteration == 1)
                            SimulateDeadlock();
                        else
                            ExecuteSql("INSERT Logs VALUES(2)");
                    });

                ExecuteSql("INSERT Logs VALUES(3)");

                transaction.Complete();
            }
        }

        private static void RetryIfDeadlock(Action act)
        {
            // https://stackoverflow.com/a/13159533/1400547
            var retryCount = 0;
            while (retryCount < 3)
                try
                {
                    act();
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

        private static void SimulateDeadlock()
        {
            // https://stackoverflow.com/a/39299800/1400547
            using (Transaction.Current == null ? new TransactionScope() : null)
            {
                ExecuteSql("IF EXISTS (SELECT * FROM sys.types WHERE name = 'IntIntSet') DROP TYPE [dbo].[IntIntSet]");
                ExecuteSql("CREATE TYPE dbo.IntIntSet AS TABLE(Value0 Int NOT NULL,Value1 Int NOT NULL)");
                ExecuteSql("DECLARE @myPK dbo.IntIntSet;");
            }
        }

        [SetUp]
        public void CreateTable() => ExecuteSql(@"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Logs' AND xtype='U')
            CREATE TABLE Logs(Id INT PRIMARY KEY NOT NULL)
            DELETE FROM Logs");

        private static void ExecuteSql(string sql)
        {
            using (var con = new SqlConnection(ConnectionString))
            {
                con.Open();
                var cmd = con.CreateCommand();
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }
    }
}