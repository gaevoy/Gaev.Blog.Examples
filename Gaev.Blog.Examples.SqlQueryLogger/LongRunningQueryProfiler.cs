using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using NLog;
using StackExchange.Profiling;
using StackExchange.Profiling.Data;
using StackExchange.Profiling.Helpers;
using DbCommandKey = System.Tuple<object, StackExchange.Profiling.Data.SqlExecuteType>;

namespace Gaev.Blog.Examples
{
    public class LongRunningQueryProfiler : MiniProfiler, IDbProfiler
    {
        private readonly TimeSpan _threshold;
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<DbCommandKey, Stopwatch> _inProgress =
            new ConcurrentDictionary<DbCommandKey, Stopwatch>();

        public LongRunningQueryProfiler(ILogger logger, TimeSpan threshold) : base(null, DefaultOptions)
        {
            _threshold = threshold;
            _logger = logger;
        }

        public void ExecuteStart(IDbCommand command, SqlExecuteType type)
        {
            DbCommandKey id = Tuple.Create((object) command, type);
            _inProgress[id] = Stopwatch.StartNew();
        }

        public void ExecuteFinish(IDbCommand command, SqlExecuteType type, DbDataReader reader)
        {
            DbCommandKey id = Tuple.Create((object) command, type);
            if (_inProgress.TryRemove(id, out Stopwatch stopwatch) && stopwatch.Elapsed > _threshold)
                _logger.Warn("{LongRunningQuery}", new
                {
                    stackTrace = StackTraceSnippet.Get(Options),
                    sql = command.CommandText,
                    elapsed = (long) stopwatch.Elapsed.TotalMilliseconds
                });
        }

        public void ReaderFinish(IDataReader reader)
        {
        }

        public void OnError(IDbCommand command, SqlExecuteType type, Exception exception)
        {
            DbCommandKey id = Tuple.Create((object) command, type);
            if (_inProgress.TryRemove(id, out Stopwatch stopwatch) && stopwatch.Elapsed > _threshold)
                _logger.Warn(exception, "{LongRunningQuery}", new
                {
                    sql = command.CommandText,
                    elapsed = (long) stopwatch.Elapsed.TotalMilliseconds
                });
        }
    }
}