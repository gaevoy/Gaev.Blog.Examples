using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NLog;
using NLog.Config;
using NLog.Targets;
using NUnit.Framework;
using Serilog;

namespace Gaev.Blog.Examples
{
    [NonParallelizable]
    public class ErrorLoggingTests
    {
        public string ConsoleOutput => _consoleOutput.ToString();
        private StringBuilder _consoleOutput;
        private TextWriter _originalConsoleOutput;

        [Test]
        public void Serilog_should_log_exception_data()
        {
            // Given
            var logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
            var entityId = "AAA";
            var correlationId = "BBB";

            // When
            try
            {
                StableTask(entityId, correlationId);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "My test fails {Data}", GetData(ex));
            }

            // Then
            Assert.That(ConsoleOutput, Does.Contain(entityId));
            Assert.That(ConsoleOutput, Does.Contain(correlationId));
        }

        [Test]
        public void NLog_should_log_exception_data()
        {
            // Given
            var logger = new LogFactory(WriteToConsoleConfig()).GetCurrentClassLogger();
            var entityId = "AAA";
            var correlationId = "BBB";

            // When
            try
            {
                StableTask(entityId, correlationId);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "My test fails {Data}", GetData(ex));
            }

            // Then
            Assert.That(ConsoleOutput, Does.Contain(entityId));
            Assert.That(ConsoleOutput, Does.Contain(correlationId));
        }

        private static Dictionary<string, object> GetData(Exception error)
        {
            var data = new Dictionary<string, object>();
            while (error != null)
            {
                foreach (string key in error.Data.Keys)
                    data[key] = error.Data[key];
                error = error.InnerException;
            }

            return data;
        }

        [SetUp]
        public void CaptureConsoleOutput()
        {
            _originalConsoleOutput = Console.Out;
            _consoleOutput = new StringBuilder();
            Console.SetOut(new StringWriter(_consoleOutput));
        }

        [TearDown]
        public void RestoreConsoleOutput()
        {
            Console.SetOut(_originalConsoleOutput);
            Console.Write(_consoleOutput);
        }

        private static LoggingConfiguration WriteToConsoleConfig()
        {
            var config = new LoggingConfiguration();
            var target = new ColoredConsoleTarget("console")
            {
                Layout = @"[${date:format=HH\:mm\:ss} ${level}] ${message}${newline}${exception:format=ToString}"
            };
            config.AddTarget(target);
            config.AddRuleForAllLevels(target);
            return config;
        }

        void StableTask(string stableTaskId, string semistableTask)
        {
            try
            {
                SemistableTask(semistableTask);
            }
            catch (Exception ex)
            {
                ex.Data["stableTaskId"] = stableTaskId;
                throw;
            }
        }

        void SemistableTask(string semistableTaskId)
        {
            try
            {
                UnstableTask();
            }
            catch (Exception ex)
            {
                ex.Data["semistableTaskId"] = semistableTaskId;
                throw new Exception("SemistableTask", ex);
            }
        }

        void UnstableTask()
        {
            throw new Exception("UnstableTask");
        }
    }
}