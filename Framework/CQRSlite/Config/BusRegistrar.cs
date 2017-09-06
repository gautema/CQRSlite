using CQRSlite.Bus;
using CQRSlite.Commands;
using CQRSlite.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace CQRSlite.Config
{
    public class BusRegistrar
    {
        private readonly IServiceLocator _serviceLocator;

        public BusRegistrar(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator ?? throw new ArgumentNullException(nameof(serviceLocator));
        }

        public void Register(params Type[] typesFromAssemblyContainingMessages)
        {
            var registrar = _serviceLocator.GetService<IHandlerRegistrar>();

            foreach (var typesFromAssemblyContainingMessage in typesFromAssemblyContainingMessages)
            {
                var executorsAssembly = typesFromAssemblyContainingMessage.GetTypeInfo().Assembly;
                var executorTypes = executorsAssembly
                    .GetTypes()
                    .Select(t => new { Type = t, Interfaces = ResolveMessageHandlerInterface(t) })
                    .Where(e => e.Interfaces != null && e.Interfaces.Any());

                foreach (var executorType in executorTypes)
                {
                    foreach (var @interface in executorType.Interfaces)
                    {
                        InvokeHandler(@interface, registrar, executorType.Type);
                    }
                }
            }
        }

        private void InvokeHandler(Type @interface, IHandlerRegistrar registrar, Type executorType)
        {
            var commandType = @interface.GetGenericArguments()[0];

            var registerExecutorMethod = registrar
                .GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(mi => mi.Name == "RegisterHandler")
                .Where(mi => mi.IsGenericMethod)
                .Where(mi => mi.GetGenericArguments().Length == 1)
                .Single(mi => mi.GetParameters().Length == 1)
                .MakeGenericMethod(commandType);

            Func<object, CancellationToken, Task> del;
            if (IsCancellable(@interface))
            {
                del = (x, token) =>
                {
                    var handler = _serviceLocator.GetService(executorType);
                    var method = handler.GetType()
                        .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Where(mi => mi.Name == "Handle")
                        .Single(mi => mi.GetParameters().Length == 2 &&
                                      mi.GetParameters()[0].ParameterType == x.GetType());
                    return (Task)method.Invoke(handler, new[] { x, token });
                };
            }
            else
            {
                del = (x, token) =>
                {
                    var handler = _serviceLocator.GetService(executorType);
                    var method = handler.GetType()
                        .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Where(mi => mi.Name == "Handle")
                        .Single(mi => mi.GetParameters().Length == 1 &&
                                      mi.GetParameters()[0].ParameterType == x.GetType());
                    return (Task)method.Invoke(handler, new[] { x });
                };
            }

            registerExecutorMethod.Invoke(registrar, new object[] { del });
        }

        private static bool IsCancellable(Type @interface)
        {
            return @interface.GetGenericTypeDefinition() == typeof(ICancellableCommandHandler<>)
                   || @interface.GetGenericTypeDefinition() == typeof(ICancellableEventHandler<>);
        }

        private static IEnumerable<Type> ResolveMessageHandlerInterface(Type type)
        {
            return type
                .GetInterfaces()
                .Where(i => i.GetTypeInfo().IsGenericType &&
                            (i.GetGenericTypeDefinition() == typeof(ICommandHandler<>)
                             || i.GetGenericTypeDefinition() == typeof(IEventHandler<>)
                             || i.GetGenericTypeDefinition() == typeof(ICancellableCommandHandler<>)
                             || i.GetGenericTypeDefinition() == typeof(ICancellableEventHandler<>)
                            ));
        }
    }
}
