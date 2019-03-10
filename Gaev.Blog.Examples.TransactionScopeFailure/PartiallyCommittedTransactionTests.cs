using System;
using System.Collections.Generic;
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
            TestDelegate act = () =>
            {
                // Given
                using (var transaction = new TransactionScope())
                {
                    // When
                    ExecuteSql("INSERT Logs VALUES(1)");
                    RetryIfDeadlock(iteration =>
                    {
                        if (iteration == 1)
                            SimulateDeadlock();
                        else
                            ExecuteSql("INSERT Logs VALUES(2)");
                    });
                    ExecuteSql("INSERT Logs VALUES(3)");
                    transaction.Complete();
                }
            };

            // Then
            Assert.Multiple(() =>
            {
                Assert.That(act, Throws.Nothing);
                Assert.That(GetLogs(), Is.EquivalentTo(new[] {1, 2, 3}));
            });
        }

        [Test]
        public void Reproduction2()
        {
            TestDelegate act = () =>
            {
                // Given
                using (var transaction = new TransactionScope())
                {
                    // When
                    ExecuteSql("INSERT Logs VALUES(1)");
                    try { SimulateDeadlock(); } catch (Exception) { }
                    ExecuteSql("INSERT Logs VALUES(2)");
                    ExecuteSql("INSERT Logs VALUES(3)");
                    transaction.Complete();
                }
            };

            // Then
            Assert.Multiple(() =>
            {
                Assert.That(act, Throws.Nothing);
                Assert.That(GetLogs(), Is.EquivalentTo(new[] {1, 2, 3}));
            });
        }

        [Test]
        public void Reproduction3()
        {
            TestDelegate act = () =>
            {
                // Given
                using (var transaction = new TransactionScope())
                {
                    // When
                    ExecuteSql("INSERT Logs VALUES(1)");
                    try { ExecuteSql("INSERT Logs VALUES('oops')"); } catch (Exception) { }
                    ExecuteSql("INSERT Logs VALUES(2)");
                    ExecuteSql("INSERT Logs VALUES(3)");
                    transaction.Complete();
                }
            };

            // Then
            Assert.Multiple(() =>
            {
                Assert.That(act, Throws.Nothing);
                Assert.That(GetLogs(), Is.EquivalentTo(new[] {1, 2, 3}));
            });
            // https://stackoverflow.com/a/5623877/1400547
        }

        [Test]
        public void Fix1()
        {
            RetryIfDeadlock(iteration =>
            {
                // Given
                using (var transaction = new TransactionScope())
                {
                    // When
                    ExecuteSql("INSERT Logs VALUES(1)");
                    if (iteration == 1)
                        SimulateDeadlock();
                    else
                        ExecuteSql("INSERT Logs VALUES(2)");
                    ExecuteSql("INSERT Logs VALUES(3)");
                    transaction.Complete();
                }
            });

            // Then
            Assert.That(GetLogs(), Is.EquivalentTo(new[] {1, 2, 3}));
        }

        [Test]
        public void Fix2()
        {
            // Given
            using (var transaction = new TransactionScope())
            {
                // When
                ExecuteSql("INSERT Logs VALUES(1)");
                using (new TransactionScope(TransactionScopeOption.Suppress))
                    RetryIfDeadlock(iteration =>
                    {
                        if (iteration == 1)
                            SimulateDeadlock();
                        else
                            ExecuteSql("INSERT Logs VALUES(2)");
                    });
                ExecuteSql("INSERT Logs VALUES(3)");
                transaction.Complete();
            }

            // Then
            Assert.That(GetLogs(), Is.EquivalentTo(new[] {1, 2, 3}));
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

        private static void RetryIfDeadlock(Action<int> act)
        {
            // https://stackoverflow.com/a/13159533/1400547
            var iteration = 1;
            while (iteration <= 3)
                try
                {
                    act(iteration);
                    break;
                }
                catch (SqlException e)
                {
                    if (e.Number == 1205)
                        iteration++;
                    else
                        throw;
                }
        }

        private static IEnumerable<int> GetLogs()
        {
            using (var con = new SqlConnection(ConnectionString))
            {
                con.Open();
                var cmd = con.CreateCommand();
                cmd.CommandText = "SELECT Id FROM Logs";
                using (var dtr = cmd.ExecuteReader())
                    while (dtr.Read())
                        yield return (int) dtr["Id"];
            }
        }
    }
}