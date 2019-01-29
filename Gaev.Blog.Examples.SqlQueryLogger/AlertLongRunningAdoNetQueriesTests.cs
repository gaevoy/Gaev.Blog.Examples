using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using StackExchange.Profiling.Data;
#pragma warning disable 168

namespace Gaev.Blog.Examples
{
    public class AlertLongRunningAdoNetQueriesTests
    {
        private const string ConnectionString = "server=localhost;database=tempdb;UID=sa;PWD=sa123";

        [Test]
        public async Task It_should_alert_ADO_NET_SQL_queries()
        {
            // Given
            var logger = new TestLogger();
            var profiler = new LongRunningQueryProfiler(logger, threshold: 100.Milliseconds());
            var connectionFactory = new DbConnectionFactory(ConnectionString, profiler);
            var con = await connectionFactory.Open();

            // When
            var cmd = con.CreateCommand();
            cmd.CommandText = "WAITFOR DELAY '00:00:00.200'; SELECT '123' as 'Test'";
            using (var dtr = await cmd.ExecuteReaderAsync())
                while (await dtr.ReadAsync())
                {
                }

            // Then
            Assert.That(logger.Warnings.Any(e => e.sql == cmd.CommandText), Is.True);
        }
        
        [Test]
        public async Task It_should_not_alert_ADO_NET_SQL_queries()
        {
            // Given
            var logger = new TestLogger();
            var profiler = new LongRunningQueryProfiler(logger, threshold: 1000.Milliseconds());
            var connectionFactory = new DbConnectionFactory(ConnectionString, profiler);
            var con = await connectionFactory.Open();

            // When
            var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT '123' as 'Test'";
            using (var dtr = await cmd.ExecuteReaderAsync())
                while (await dtr.ReadAsync())
                {
                }

            // Then
            Assert.That(logger.Warnings, Is.Empty);
        }

        [Test]
        public async Task It_should_alert_ADO_NET_SQL_queries_with_error()
        {
            // Given
            var logger = new TestLogger();
            var profiler = new LongRunningQueryProfiler(logger, threshold: 100.Milliseconds());
            var connectionFactory = new DbConnectionFactory(ConnectionString, profiler);
            var con = await connectionFactory.Open();

            // When
            var cmd = con.CreateCommand();
            cmd.CommandTimeout = 1;
            cmd.CommandText = "WAITFOR DELAY '00:00:02'";
            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex)
            {
                // Ignore it
            }

            // Then
            Assert.That(logger.Warnings.Any(e => e.sql == cmd.CommandText), Is.True);
        }

        [Test]
        public async Task It_should_alert_ADO_NET_SQL_queries_with_syntax_error()
        {
            // Given
            var logger = new TestLogger();
            var profiler = new LongRunningQueryProfiler(logger, threshold: 100.Milliseconds());
            var connectionFactory = new DbConnectionFactory(ConnectionString, profiler);
            var con = await connectionFactory.Open();

            // When
            var cmd = con.CreateCommand();
            cmd.CommandTimeout = 1;
            cmd.CommandText = "WAITFOR DELAY '00:00:00.200'; EXECUTE sp_executesql N'BOO!';";
            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex)
            {
                // Ignore it
            }

            // Then
            Assert.That(logger.Warnings.Any(e => e.sql == cmd.CommandText), Is.True);
        }
        
        public class DbConnectionFactory
        {
            private readonly string _connectionString;
            private readonly IDbProfiler _profiler;

            public DbConnectionFactory(string connectionString, IDbProfiler profiler)
            {
                _connectionString = connectionString;
                _profiler = profiler;
            }
        
            public async Task<DbConnection> Open()
            {
                var connection = new ProfiledDbConnection(new SqlConnection(_connectionString), _profiler);
                await connection.OpenAsync();
                return connection;
            }
        }
    }
}