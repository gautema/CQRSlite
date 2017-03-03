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
        private readonly Dictionary<Type, List<Action<IMessage>>> _routes = new Dictionary<Type, List<Action<IMessage>>>();

        public void RegisterHandler<T>(Action<T> handler) where T : IMessage
        {
            List<Action<IMessage>> handlers;
            if (!_routes.TryGetValue(typeof(T), out handlers))
            {
                handlers = new List<Action<IMessage>>();
                _routes.Add(typeof(T), handlers);
            }
            handlers.Add((x => handler((T)x)));
        }

        public Task Send<T>(T command) where T : ICommand
        {
            List<Action<IMessage>> handlers;
            if (!_routes.TryGetValue(command.GetType(), out handlers))
                throw new InvalidOperationException("No handler registered");
            if (handlers.Count != 1)
                throw new InvalidOperationException("Cannot send to more than one handler");

            handlers[0](command);
            return Task.CompletedTask;
        }

        public Task Publish<T>(T @event) where T : IEvent
        {
            List<Action<IMessage>> handlers;
            if (!_routes.TryGetValue(@event.GetType(), out handlers))
                return Task.CompletedTask;
            return Task.WhenAll(handlers.Select(x =>
            {
                x(@event);
                return Task.CompletedTask;
            }));
        }
    }
}
