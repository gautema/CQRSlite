using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CQRSlite.Infrastructure;
using CQRSlite.Messages;
using CQRSlite.Routing.Exception;

namespace CQRSlite.Routing
{
    /// <summary>
    /// Automatic registration of all handlers in assembly
    /// </summary>
    public class RouteRegistrar
    {
        private readonly IServiceProvider _serviceLocator;

        /// <summary>
        /// Initialize a new instance of <cref>RouteRegister</cref> class.
        /// </summary>
        /// <param name="serviceLocator">Service locator that can resolve all handlers</param>
        public RouteRegistrar(IServiceProvider serviceLocator)
        {
            _serviceLocator = serviceLocator ?? throw new ArgumentNullException(nameof(serviceLocator));
        }

        /// <summary>
        /// Register all command and event handlers in assembly
        /// </summary>
        /// <param name="typesFromAssemblyContainingMessages">List of assemblies to scan for handlers.</param>
        public void Register(params Type[] typesFromAssemblyContainingMessages)
        {
            var registrar = (IHandlerRegistrar)_serviceLocator.GetService(typeof(IHandlerRegistrar));

            foreach (var typesFromAssemblyContainingMessage in typesFromAssemblyContainingMessages)
            {
                var executorsAssembly = typesFromAssemblyContainingMessage.GetTypeInfo().Assembly;
                var executorTypes = executorsAssembly
                    .GetTypes()
                    .Select(t => new { Type = t, Interfaces = ResolveMessageHandlerInterface(t) })
                    .Where(e => e.Interfaces != null && e.Interfaces.Any() && !e.Type.GetTypeInfo().IsAbstract);

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


            Func<object, CancellationToken, Task> func;
            if (IsCancellable(@interface))
            {
                var methodname = executorType.GetImplementationNameOfInterfaceMethod(@interface, "Handle", commandType,
                    typeof(CancellationToken));

                func = (@event, token) =>
                {
                    var handler = _serviceLocator.GetService(executorType) ?? 
                        throw new HandlerNotResolvedException(executorType.Name);
                    return (Task) (handler.Invoke(methodname, @event, token) ??
                                   throw new ResolvedHandlerMethodNotFoundException(executorType.Name));
                };
            }
            else
            {
                var methodname = executorType.GetImplementationNameOfInterfaceMethod(@interface, "Handle", commandType);
                func = (@event, token) =>
                {
                    var handler = _serviceLocator.GetService(executorType) ?? 
                        throw new HandlerNotResolvedException(executorType.Name);
                    return (Task) (handler.Invoke(methodname, @event) ??
                                   throw new ResolvedHandlerMethodNotFoundException(executorType.Name));
                };
            }

            registerExecutorMethod.Invoke(registrar, new object[] { func });
        }

        private static bool IsCancellable(Type @interface)
        {
            return @interface.GetGenericTypeDefinition() == typeof(ICancellableHandler<>);
        }

        private static IEnumerable<Type> ResolveMessageHandlerInterface(Type type)
        {
            return type
                .GetInterfaces()
                .Where(i => i.GetTypeInfo().IsGenericType &&
                            (i.GetGenericTypeDefinition() == typeof(IHandler<>)
                             || i.GetGenericTypeDefinition() == typeof(ICancellableHandler<>)
                            ));
        }
    }
}
