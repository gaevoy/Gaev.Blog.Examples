using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NLog;

namespace Gaev.Blog.Examples
{
    public class TestLogger : Logger, ILogger
    {
        public readonly List<dynamic> Warnings = new List<dynamic>();

        void ILogger.Warn<TArgument>(string message, TArgument argument)
        {
            lock (Warnings)
                Warnings.Add(JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(argument)));
        }

        void ILogger.Warn(Exception _, string message, params object[] args)
        {
            lock (Warnings)
                Warnings.Add(JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(args[0])));
        }
    }
}