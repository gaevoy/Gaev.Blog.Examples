using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gaev.Blog.Examples.SelfDocumentedFSM
{
    public interface IStateMachine
    {
        Task<TResult> HandleAsync<TMessage, TResult>(TMessage msg);
        void Become(Action state);
        void ReceiveAsync<TMessage, TResult>(Func<TMessage, Task<TResult>> handler);
        void ReceiveAnyAsync(Func<object, Task<object>> handler);
    }
    public sealed class StateMachine : IStateMachine
    {
        private Func<object, Task<object>> _any = Void.ReturnNothingAsync;
        private Dictionary<Type, object> _handlers = new Dictionary<Type, object>();
        public async Task<TResult> HandleAsync<TMessage, TResult>(TMessage msg)
        {
            object handler;
            if (_handlers.TryGetValue(typeof(TMessage), out handler))
                return await ((Func<TMessage, Task<TResult>>) handler)(msg);
            return (TResult) await _any(msg);
        }
        public void Become(Action state)
        {
            _any = Void.ReturnNothingAsync;
            _handlers = new Dictionary<Type, object>();
            state();
        }
        public void ReceiveAsync<TMessage, TResult>(Func<TMessage, Task<TResult>> handler)
        {
            _handlers[typeof(TMessage)] = handler;
        }
        public void ReceiveAnyAsync(Func<object, Task<object>> handler)
        {
            _any = handler;
        }
    }
    public sealed class Void
    {
        private Void()
        {
        }
        public static readonly Task<object> CompletedObjectTask = Task.FromResult<object>(null);
        public static readonly Void Nothing = new Void();
        public static Task<object> ReturnNothingAsync(object _) => CompletedObjectTask;
    }
}