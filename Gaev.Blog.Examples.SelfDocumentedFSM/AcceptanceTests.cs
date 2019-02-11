using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Serilog;

namespace Gaev.Blog.Examples.SelfDocumentedFSM
{
    [TestFixture]
    public class AcceptanceTests
    {
        [Test]
        public void Test1()
        {
            Assert.True(true);
        }
    }

    public class PlantUmlDiagramBuilder : IStateMachine
    {
        private readonly IStateMachine _underlying;
        private readonly ILogger _logger;
        private readonly Func<string> _getStateName;
        private string _action;

        public PlantUmlDiagramBuilder(IStateMachine underlying, ILogger logger, Func<string> getStateName)
        {
            _underlying = underlying;
            _logger = logger;
            _getStateName = getStateName;
        }

        public Task<TResult> HandleAsync<TMessage, TResult>(TMessage msg)
        {
            _action = msg.GetType().Name;
            return _underlying.HandleAsync<TMessage, TResult>(msg);
        }

        public void Become(Action state)
        {
            var from = _getStateName();
            _underlying.Become(state);
            var to = _getStateName();
            var comment = _action == null ? "" : ":" + _action;
            _logger.Debug($"{from ?? "[*]"} --> {to} {comment}");
        }

        public void ReceiveAsync<TMessage, TResult>(Func<TMessage, Task<TResult>> handler)
            => _underlying.ReceiveAsync(handler);

        public void ReceiveAnyAsync(Func<object, Task<object>> handler)
            => _underlying.ReceiveAnyAsync(handler);
    }
}