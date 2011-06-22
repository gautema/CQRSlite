using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CQRSlite.Bus;

namespace CQRSlite.Config
{
    public class BusRegisterer
    {
        private readonly IServiceLocator _serviceLocator;
        public BusRegisterer(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
        }

        public void Register(params Type[] typesFromAssemblyContainingMessages)
        {
            var bus = _serviceLocator.GetService<IHandleRegister>();
            
            foreach (var typesFromAssemblyContainingMessage in typesFromAssemblyContainingMessages)
            {
                var executorsAssembly = typesFromAssemblyContainingMessage.Assembly;
                var executorTypes = executorsAssembly
                    .GetTypes()
                    .Select(t => new { Type = t, Interfaces = ResolveMessageHandlerInterface(t) })
                    .Where(e => e.Interfaces != null && e.Interfaces.Count() > 0);

                foreach (var executorType in executorTypes)
                    foreach (var @interface in executorType.Interfaces)
                        InvokeHandler(@interface, bus, executorType.Type);
            }
        }

        private void InvokeHandler(Type @interface, IHandleRegister bus, Type executorType) {
            var commandType = @interface.GetGenericArguments()[0];

            var registerExecutorMethod = bus
                .GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(mi => mi.Name == "RegisterHandler")
                .Where(mi => mi.IsGenericMethod)
                .Where(mi => mi.GetGenericArguments().Count() == 1)
                .Where(mi => mi.GetParameters().Count() == 1)
                .Single()
                .MakeGenericMethod(commandType);

            var del = new Action<dynamic>(x =>
                                              {
                                                  dynamic handler = _serviceLocator.GetService(executorType);
                                                  handler.Handle(x);
                                              });
            
            registerExecutorMethod.Invoke(bus, new[] { del });
        }

        private static IEnumerable<Type> ResolveMessageHandlerInterface(Type type)
        {
            return type
                .GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandles<>));
        }

    }
}
