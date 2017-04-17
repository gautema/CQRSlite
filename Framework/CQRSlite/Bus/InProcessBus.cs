using CQRSlite.Commands;
using CQRSlite.Events;
using CQRSlite.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQRSlite.Bus
{
    public class InProcessBus : ICommandSender, IEventPublisher, IHandlerRegistrar
    {
        private readonly Dictionary<Type, List<Func<IMessage, Task>>> _routes = new Dictionary<Type, List<Func<IMessage, Task>>>();

        public void RegisterHandler<T>(Func<T,Task> handler) where T : class, IMessage
        {
            if (!_routes.TryGetValue(typeof(T), out var handlers))
            {
                handlers = new List<Func<IMessage, Task>>();
                _routes.Add(typeof(T), handlers);
            }
            handlers.Add(x => handler((T)x));
        }

        public Task Send<T>(T command) where T : class, ICommand
        {
            if (!_routes.TryGetValue(command.GetType(), out var handlers))
                throw new InvalidOperationException("No handler registered");
            if (handlers.Count != 1)
                throw new InvalidOperationException("Cannot send to more than one handler");
            return handlers[0](command);
        }

        public Task Publish<T>(T @event) where T : class, IEvent
        {
            if (!_routes.TryGetValue(@event.GetType(), out var handlers))
                return Task.CompletedTask;
            return Task.WhenAll(handlers.Select(handler => handler(@event)));
        }
    }
}
