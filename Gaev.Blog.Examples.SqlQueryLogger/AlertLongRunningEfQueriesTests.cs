using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using StackExchange.Profiling;
using StackExchange.Profiling.EntityFramework6;

namespace Gaev.Blog.Examples
{
    [NonParallelizable]
    public class AlertLongRunningEfQueriesTests
    {
        private const string ConnectionString = "server=localhost;database=tempdb;UID=sa;PWD=sa123";
        private TestLogger _logger;

        [OneTimeSetUp]
        public void InitializeEf6Profiler()
        {
            _logger = new TestLogger();
            var profiler = new LongRunningQueryProfiler(_logger, threshold: 200.Milliseconds());
            MiniProfiler.DefaultOptions.ProfilerProvider = new ProfilerGetter(profiler);
            MiniProfilerEF6.Initialize();
        }

        [Test]
        public async Task It_should_alert_EntityFramework_SQL_queries()
        {
            // Given
            _logger.Warnings.Clear();

            // When
            var sql = "WAITFOR DELAY '00:00:00.200'; SELECT 123 as 'Id'";
            using (var ctx = new MyDbContext(ConnectionString))
            {
                var _ = await ctx.Cars.SqlQuery(sql).ToListAsync();
            }

            // Then
            Assert.That(_logger.Warnings.Any(e => e.sql == sql), Is.True);
        }

        [Test]
        public async Task It_should_not_alert_EntityFramework_SQL_queries()
        {
            // Given
            _logger.Warnings.Clear();

            // When
            var sql = "SELECT 123 as 'Id'";
            using (var ctx = new MyDbContext(ConnectionString))
            {
                var _ = await ctx.Cars.SqlQuery(sql).ToListAsync();
            }

            // Then
            Assert.That(_logger.Warnings, Is.Empty);
        }

        public class MyDbContext : DbContext
        {
            public DbSet<Car> Cars { get; set; }

            static MyDbContext()
            {
                Database.SetInitializer<MyDbContext>(null);
            }

            public MyDbContext(string connectionString) : base(connectionString)
            {
            }
        }

        public class Car
        {
            public int Id { get; set; }
        }
    }
}